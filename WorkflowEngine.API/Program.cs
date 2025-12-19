using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WorkflowEngine.Infrastructure.Data;
using WorkflowEngine.Infrastructure.Security;
using WorkflowEngine.Infrastructure.Services;
using WorkflowEngine.Core.Interfaces;
using WorkflowEngine.API.Middlewares;
using Serilog;
using WorkflowEngine.Infrastructure.Hubs;
using Hangfire;
using Hangfire.MemoryStorage;
using WorkflowEngine.Infrastructure.Jobs;

var builder = WebApplication.CreateBuilder(args);

// SERILOG CONFIGURATION (FAZ 6)
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("Logs/workflow-log-.txt", rollingInterval: RollingInterval.Day));

// --- 1. SERVİS KAYITLARI ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var xmlFilename = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(System.IO.Path.Combine(AppContext.BaseDirectory, xmlFilename));

    options.EnableAnnotations();
    options.CustomSchemaIds(type => type.FullName);
    options.CustomOperationIds(e => $"{e.ActionDescriptor.RouteValues["controller"]}_{e.ActionDescriptor.RouteValues["action"]}");

    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and then your valid token in the text input below.\r\n\r\nExample: \"Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...\""
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});
builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddMemoryCache();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? httpContext.Request.Headers.Host.ToString(),
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 10,
                QueueLimit = 5,
                Window = TimeSpan.FromSeconds(1)
            }));
    options.RejectionStatusCode = 429;
});

// HANGFIRE CONFIGURATION (FAZ 6.5 Part 2)
builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseMemoryStorage());
builder.Services.AddHangfireServer();

// GÜVENLİK SERVİSLERİ (FAZ 2)
builder.Services.AddSingleton<IMachineIdGenerator, MachineIdGenerator>();
builder.Services.AddScoped<ILicenseValidator, LicenseValidator>();
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

// WORKFLOW ENGINE SERVİSLERİ (FAZ 3)
builder.Services.AddScoped<IWorkflowService, WorkflowService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();

// ENTEGRASYON SERVİSLERİ (FAZ 6.5)
builder.Services.AddScoped<IStorageService, LocalDiskStorageService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IDocumentService, DocumentService>();

// JOBS
builder.Services.AddScoped<TimeoutCheckerJob>();

// JWT AUTHENTICATION
var jwtKey = builder.Configuration["JwtSettings:Key"];
if (!string.IsNullOrEmpty(jwtKey))
{
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ClockSkew = TimeSpan.Zero
        };
    });
}

// VERİTABANI AYARLARI (FAZ 1)
var dbProvider = builder.Configuration.GetValue<string>("DbProvider");
var connectionString = "";

if (string.Equals(dbProvider, "PostgreSQL", StringComparison.OrdinalIgnoreCase))
{
    connectionString = builder.Configuration.GetConnectionString("PostgreSQL");
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(connectionString));
}
else if (string.Equals(dbProvider, "MSSQL", StringComparison.OrdinalIgnoreCase))
{
    connectionString = builder.Configuration.GetConnectionString("MSSQL");
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(connectionString));
}
else
{
    throw new Exception($"Unsupported DbProvider: {dbProvider}");
}

var app = builder.Build();

// --- 2. HTTP PIPELINE ---

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// SERILOG REQUEST LOGGING
app.UseSerilogRequestLogging();

// HANGFIRE DASHBOARD
app.UseHangfireDashboard("/hangfire");

// GÜVENLİK DUVARI (Middleware)
// Controller'lardan önce çalışması şart!
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseMiddleware<LicenseCheckMiddleware>();

app.UseCors("AllowAll");
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<NotificationHub>("/hub/notifications");

// Seed Database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        // Automatically apply migrations (Optional, but good for dev)
        // context.Database.Migrate();
        DbInitializer.Initialize(context);

        // Schedule Recurring Jobs
        var recurringJobManager = services.GetRequiredService<IRecurringJobManager>();
        recurringJobManager.AddOrUpdate<TimeoutCheckerJob>("check-timeouts", job => job.CheckTimeoutsAsync(), Cron.Hourly);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred seeding the DB or scheduling jobs.");
    }
}

app.Run();

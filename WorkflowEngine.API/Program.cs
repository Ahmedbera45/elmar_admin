using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WorkflowEngine.Infrastructure.Data;
using WorkflowEngine.Infrastructure.Security;
using WorkflowEngine.Infrastructure.Services;
using WorkflowEngine.Core.Interfaces;
using WorkflowEngine.API.Middlewares;
using Serilog;
using WorkflowEngine.Infrastructure.Hubs;

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
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddSignalR(); // Add SignalR

// GÜVENLİK SERVİSLERİ (FAZ 2)
builder.Services.AddSingleton<IMachineIdGenerator, MachineIdGenerator>();
builder.Services.AddScoped<ILicenseValidator, LicenseValidator>();
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

// WORKFLOW ENGINE SERVİSLERİ (FAZ 3)
builder.Services.AddScoped<IWorkflowService, WorkflowService>();

// ENTEGRASYON SERVİSLERİ (FAZ 6.5)
builder.Services.AddScoped<IStorageService, LocalDiskStorageService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

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

// GÜVENLİK DUVARI (Middleware)
// Controller'lardan önce çalışması şart!
app.UseMiddleware<LicenseCheckMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<NotificationHub>("/hub/notifications"); // Map SignalR Hub

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
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred seeding the DB.");
    }
}

app.Run();

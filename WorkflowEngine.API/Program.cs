using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WorkflowEngine.Infrastructure.Data;
using WorkflowEngine.Infrastructure.Security;
using WorkflowEngine.Core.Interfaces;
using WorkflowEngine.API.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// --- 1. SERVİS KAYITLARI ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

// GÜVENLİK SERVİSLERİ (FAZ 2)
builder.Services.AddSingleton<IMachineIdGenerator, MachineIdGenerator>();
builder.Services.AddScoped<ILicenseValidator, LicenseValidator>();
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

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

// GÜVENLİK DUVARI (Middleware)
// Controller'lardan önce çalışması şart!
app.UseMiddleware<LicenseCheckMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

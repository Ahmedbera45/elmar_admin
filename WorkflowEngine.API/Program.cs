using Microsoft.EntityFrameworkCore;
using WorkflowEngine.API.Middlewares;
using WorkflowEngine.Core.Interfaces;
using WorkflowEngine.Infrastructure.Data;
using WorkflowEngine.Infrastructure.Security;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Core Services
// MachineID should be Singleton as hardware doesn't change per request
builder.Services.AddSingleton<IMachineIdGenerator, MachineIdGenerator>();
// LicenseValidator can be Scoped or Transient
builder.Services.AddScoped<ILicenseValidator, LicenseValidator>();

// Database Strategy
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

// Configure the HTTP request pipeline.
// Middleware order matters. License check should ideally be early.
app.UseMiddleware<LicenseCheckMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.Run();

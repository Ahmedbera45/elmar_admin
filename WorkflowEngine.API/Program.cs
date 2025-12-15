using Microsoft.EntityFrameworkCore;
using WorkflowEngine.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.Run();

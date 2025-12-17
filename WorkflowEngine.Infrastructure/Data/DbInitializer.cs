using System;
using System.Linq;
using WorkflowEngine.Core.Entities;

namespace WorkflowEngine.Infrastructure.Data;

public static class DbInitializer
{
    public static void Initialize(AppDbContext context)
    {
        // Ensure database is created (optional if using migrations, but helpful for quick start)
        // context.Database.EnsureCreated();

        // Check if any users exist
        if (context.WebUsers.Any())
        {
            return; // DB has been seeded
        }

        var adminUser = new WebUser
        {
            Username = "admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("12345"),
            Email = "admin@belediye.gov.tr",
            Role = "Admin",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "System"
        };

        context.WebUsers.Add(adminUser);
        context.SaveChanges();
    }
}

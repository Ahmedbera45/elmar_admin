using Microsoft.EntityFrameworkCore;
using WorkflowEngine.Core.Common;
using WorkflowEngine.Core.Entities;

namespace WorkflowEngine.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Process> Processes { get; set; }
    public DbSet<ProcessStep> ProcessSteps { get; set; }
    public DbSet<ProcessAction> ProcessActions { get; set; }
    public DbSet<ProcessActionCondition> ProcessActionConditions { get; set; }
    public DbSet<WebUser> WebUsers { get; set; }
    public DbSet<ProcessEntry> ProcessEntries { get; set; }
    public DbSet<ProcessEntryHistory> ProcessEntryHistories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // WebUser Configuration
        modelBuilder.Entity<WebUser>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Username).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Username).IsUnique();
            entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
            entity.Property(e => e.PasswordHash).IsRequired();
        });

        // Process Configuration
        modelBuilder.Entity<Process>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.Code).IsUnique(); // Ensure Code is unique

            entity.HasMany(e => e.Steps)
                  .WithOne(s => s.Process)
                  .HasForeignKey(s => s.ProcessId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ParentProcess)
                  .WithMany()
                  .HasForeignKey(e => e.ParentProcessId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ProcessStep Configuration
        modelBuilder.Entity<ProcessStep>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.HasMany(e => e.Actions)
                  .WithOne(a => a.ProcessStep)
                  .HasForeignKey(a => a.ProcessStepId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ProcessAction Configuration
        modelBuilder.Entity<ProcessAction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);

            entity.HasOne(e => e.TargetStep)
                  .WithMany()
                  .HasForeignKey(e => e.TargetStepId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.TimeoutAction)
                  .WithMany()
                  .HasForeignKey(e => e.TimeoutActionId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.DefaultCondition)
                  .WithOne() // One-to-One (or many-to-one if modeled that way, but here ID is on Action)
                  .HasForeignKey<ProcessAction>(e => e.DefaultConditionId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(e => e.Conditions)
                  .WithOne(c => c.ProcessAction)
                  .HasForeignKey(c => c.ProcessActionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ProcessActionCondition Configuration
        modelBuilder.Entity<ProcessActionCondition>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RuleExpression).IsRequired().HasMaxLength(1000);

            entity.HasOne(e => e.TargetStep)
                  .WithMany()
                  .HasForeignKey(e => e.TargetStepId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ProcessEntry Configuration
        modelBuilder.Entity<ProcessEntry>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EntryNumber).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.EntryNumber).IsUnique();

            entity.HasOne(e => e.Process)
                  .WithMany()
                  .HasForeignKey(e => e.ProcessId)
                  .OnDelete(DeleteBehavior.Restrict); // Keep log even if definition changes/deleted? Or Cascade? Usually Restrict for historical data.

            entity.HasOne(e => e.CurrentStep)
                  .WithMany()
                  .HasForeignKey(e => e.CurrentStepId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.InitiatorUser)
                  .WithMany()
                  .HasForeignKey(e => e.InitiatorUserId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ProcessEntryHistory Configuration
        modelBuilder.Entity<ProcessEntryHistory>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasOne(e => e.ProcessEntry)
                  .WithMany() // Can define collection in ProcessEntry if needed
                  .HasForeignKey(e => e.ProcessEntryId)
                  .OnDelete(DeleteBehavior.Cascade); // If Entry is deleted, history goes too.

            entity.HasOne(e => e.FromStep)
                  .WithMany()
                  .HasForeignKey(e => e.FromStepId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ToStep)
                  .WithMany()
                  .HasForeignKey(e => e.ToStepId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Action)
                  .WithMany()
                  .HasForeignKey(e => e.ActionId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ActorUser)
                  .WithMany()
                  .HasForeignKey(e => e.ActorUserId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }

    public override int SaveChanges()
    {
        UpdateAuditFields();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditFields();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateAuditFields()
    {
        var entries = ChangeTracker.Entries<BaseEntity>();

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
                entry.Entity.CreatedBy = "System"; // TODO: Implement User Context
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.ModifiedAt = DateTime.UtcNow;
                entry.Entity.ModifiedBy = "System"; // TODO: Implement User Context
            }
        }
    }
}

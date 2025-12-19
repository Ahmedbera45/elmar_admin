using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

    // Renamed ProcessEntry -> ProcessRequests
    public DbSet<ProcessRequest> ProcessRequests { get; set; }
    // Renamed ProcessEntryHistory -> ProcessRequestHistories
    public DbSet<ProcessRequestHistory> ProcessRequestHistories { get; set; }

    // New Phase 4 Form Builder DbSets
    public DbSet<ProcessEntry> ProcessEntries { get; set; } // Form Field Definitions
    public DbSet<PePsConnection> PePsConnections { get; set; }
    public DbSet<ProcessRequestValue> ProcessRequestValues { get; set; }

    // Phase 6.5 File Storage
    public DbSet<FileMetadata> FileMetadatas { get; set; }

    // Phase 7: Security and Dynamic Views
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<ProcessListView> ProcessListViews { get; set; }
    public DbSet<ProcessListViewColumn> ProcessListViewColumns { get; set; }

    // Phase 9: Document, Notification, Audit
    public DbSet<ProcessDocumentTemplate> ProcessDocumentTemplates { get; set; }
    public DbSet<NotificationTemplate> NotificationTemplates { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<AppRole> AppRoles { get; set; }
    public DbSet<SystemSetting> SystemSettings { get; set; }

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

        // ProcessRequest Configuration (Renamed from ProcessEntry)
        modelBuilder.Entity<ProcessRequest>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RequestNumber).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.RequestNumber).IsUnique();

            entity.HasOne(e => e.Process)
                  .WithMany()
                  .HasForeignKey(e => e.ProcessId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.CurrentStep)
                  .WithMany()
                  .HasForeignKey(e => e.CurrentStepId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.InitiatorUser)
                  .WithMany()
                  .HasForeignKey(e => e.InitiatorUserId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ProcessRequestHistory Configuration (Renamed from ProcessEntryHistory)
        modelBuilder.Entity<ProcessRequestHistory>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasOne(e => e.ProcessRequest)
                  .WithMany()
                  .HasForeignKey(e => e.ProcessRequestId)
                  .OnDelete(DeleteBehavior.Cascade);

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

        // Phase 4: Form Builder Configuration

        // ProcessEntry (Form Field Definition)
        modelBuilder.Entity<ProcessEntry>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Key).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.HasIndex(e => e.Key);
        });

        // PePsConnection (Step-Field Connection)
        modelBuilder.Entity<PePsConnection>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasOne(e => e.ProcessStep)
                  .WithMany()
                  .HasForeignKey(e => e.ProcessStepId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ProcessEntry)
                  .WithMany()
                  .HasForeignKey(e => e.ProcessEntryId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ProcessRequestValue (Answers)
        modelBuilder.Entity<ProcessRequestValue>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasOne(e => e.ProcessRequest)
                  .WithMany()
                  .HasForeignKey(e => e.ProcessRequestId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ProcessEntry)
                  .WithMany()
                  .HasForeignKey(e => e.ProcessEntryId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Phase 6.5 File Metadata
        modelBuilder.Entity<FileMetadata>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.StoredFileName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.OriginalFileName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.ContentType).IsRequired().HasMaxLength(100);
        });

        // Phase 7: RefreshToken
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Token).IsRequired().HasMaxLength(200);
            entity.Property(e => e.JwtId).IsRequired().HasMaxLength(100);

            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Phase 7: ProcessListView
        modelBuilder.Entity<ProcessListView>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);

            entity.HasOne(e => e.Process)
                  .WithMany()
                  .HasForeignKey(e => e.ProcessId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Phase 7: ProcessListViewColumn
        modelBuilder.Entity<ProcessListViewColumn>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.ProcessEntryId).IsRequired().HasMaxLength(100);

            entity.HasOne(e => e.ListView)
                  .WithMany()
                  .HasForeignKey(e => e.ListViewId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Phase 9: Templates
        modelBuilder.Entity<ProcessDocumentTemplate>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);

            entity.HasOne(e => e.Process)
                  .WithMany()
                  .HasForeignKey(e => e.ProcessId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Step)
                  .WithMany()
                  .HasForeignKey(e => e.StepId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<NotificationTemplate>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Key).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Key).IsUnique();
        });

        modelBuilder.Entity<SystemSetting>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Key).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Key).IsUnique();
        });

        modelBuilder.Entity<AppRole>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Name).IsUnique();
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TableName).HasMaxLength(100);
            entity.Property(e => e.RecordId).HasMaxLength(100);
            entity.Property(e => e.Action).HasMaxLength(20);
        });
    }

    public override int SaveChanges()
    {
        UpdateAuditFields();
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditFields();
        await AuditChanges();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private async Task AuditChanges()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted)
            .ToList();

        foreach (var entry in entries)
        {
            if (entry.Entity is AuditLog) continue;

            var audit = new AuditLog
            {
                Id = Guid.NewGuid(),
                TableName = entry.Entity.GetType().Name,
                Timestamp = DateTime.UtcNow,
                Action = entry.State.ToString()
            };

            var oldValues = new Dictionary<string, object?>();
            var newValues = new Dictionary<string, object?>();

            foreach (var property in entry.Properties)
            {
                if (property.IsTemporary) continue;

                string propName = property.Metadata.Name;

                if (entry.State == EntityState.Added)
                {
                    newValues[propName] = property.CurrentValue;
                }
                else if (entry.State == EntityState.Deleted)
                {
                    oldValues[propName] = property.OriginalValue;
                }
                else if (entry.State == EntityState.Modified)
                {
                    if (property.IsModified)
                    {
                        oldValues[propName] = property.OriginalValue;
                        newValues[propName] = property.CurrentValue;
                    }
                }
            }

            audit.OldValues = oldValues.Count == 0 ? null : System.Text.Json.JsonSerializer.Serialize(oldValues);
            audit.NewValues = newValues.Count == 0 ? null : System.Text.Json.JsonSerializer.Serialize(newValues);

            var idProp = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "Id");
            if (idProp != null)
            {
                 audit.RecordId = idProp.CurrentValue?.ToString() ?? "Pending";
            }

            this.AuditLogs.Add(audit);
        }
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

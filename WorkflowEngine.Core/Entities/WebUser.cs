using System;
using WorkflowEngine.Core.Common;

namespace WorkflowEngine.Core.Entities;

public class WebUser : BaseEntity
{
    public required string Username { get; set; }
    public required string PasswordHash { get; set; }
    public required string Email { get; set; }
    public required string Role { get; set; } = "User"; // Default role
    public string? Permissions { get; set; } // Phase 6.5 Part 2
    public bool IsActive { get; set; } = true;

    public Guid? DelegateUserId { get; set; }
    public DateTime? DelegateUntil { get; set; }
    public string? NotificationPreferences { get; set; }
}

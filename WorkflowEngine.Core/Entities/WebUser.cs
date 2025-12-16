using System;
using WorkflowEngine.Core.Common;

namespace WorkflowEngine.Core.Entities;

public class WebUser : BaseEntity
{
    public required string Username { get; set; }
    public required string PasswordHash { get; set; }
    public required string Email { get; set; }
    public required string Role { get; set; } = "User"; // Default role
    public bool IsActive { get; set; } = true;
}

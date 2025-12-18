using System;

namespace WorkflowEngine.Core.Entities;

public class RefreshToken : BaseEntity
{
    public string Token { get; set; } = string.Empty;
    public string JwtId { get; set; } = string.Empty; // Jti
    public DateTime CreationDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public bool Used { get; set; }
    public bool Invalidated { get; set; }
    public Guid UserId { get; set; }

    // Navigation property
    public virtual WebUser User { get; set; } = null!;
}

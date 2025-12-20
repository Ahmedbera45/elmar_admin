using WorkflowEngine.Core.Common;

namespace WorkflowEngine.Core.Entities;

public class NotificationTemplate : BaseEntity
{
    public required string Key { get; set; }
    public required string Subject { get; set; }
    public required string Body { get; set; }
    public string? Description { get; set; }
    public Guid? ProcessActionId { get; set; }
}

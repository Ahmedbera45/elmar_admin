using WorkflowEngine.Core.Common;

namespace WorkflowEngine.Core.Entities;

public class NotificationTemplate : BaseEntity
{
    public required string Key { get; set; }
    public required string Name { get; set; }
    public required string SubjectTemplate { get; set; }
    public required string BodyTemplate { get; set; }
    public string? Description { get; set; }
    public Guid? ProcessActionId { get; set; }
}

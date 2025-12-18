using System;
using WorkflowEngine.Core.Enums;

namespace WorkflowEngine.Core.Entities;

public class NotificationTemplate : BaseEntity
{
    public Guid ProcessActionId { get; set; }
    public NotificationChannelType ChannelType { get; set; }
    public string SubjectTemplate { get; set; } = string.Empty;
    public string BodyTemplate { get; set; } = string.Empty;

    public virtual ProcessAction ProcessAction { get; set; } = null!;
}

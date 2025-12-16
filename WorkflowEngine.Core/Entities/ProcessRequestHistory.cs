using System;
using WorkflowEngine.Core.Common;

namespace WorkflowEngine.Core.Entities;

public class ProcessRequestHistory : BaseEntity
{
    public Guid ProcessRequestId { get; set; }
    public Guid? FromStepId { get; set; }
    public Guid? ToStepId { get; set; }
    public Guid? ActionId { get; set; }
    public Guid ActorUserId { get; set; }
    public DateTime ActionTime { get; set; }
    public string? Comments { get; set; }

    // Navigation Properties
    public ProcessRequest ProcessRequest { get; set; } = null!;
    public ProcessStep? FromStep { get; set; }
    public ProcessStep? ToStep { get; set; }
    public ProcessAction? Action { get; set; }
    public WebUser ActorUser { get; set; } = null!;
}

using System;
using WorkflowEngine.Core.Common;
using WorkflowEngine.Core.Enums;

namespace WorkflowEngine.Core.Entities;

public class ProcessRequest : BaseEntity
{
    public Guid ProcessId { get; set; }
    public Guid CurrentStepId { get; set; }
    public ProcessRequestStatus Status { get; set; }
    public Guid InitiatorUserId { get; set; }
    public required string RequestNumber { get; set; }
    public DateTime? DueDate { get; set; }

    // Navigation Properties
    public Process Process { get; set; } = null!;
    public ProcessStep CurrentStep { get; set; } = null!;
    public WebUser InitiatorUser { get; set; } = null!;
}

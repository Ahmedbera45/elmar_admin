using System;
using System.Collections.Generic;
using WorkflowEngine.Core.Common;
using WorkflowEngine.Core.Enums;

namespace WorkflowEngine.Core.Entities;

public class ProcessStep : BaseEntity
{
    public Guid ProcessId { get; set; }
    public required string Name { get; set; }
    public ProcessStepType StepType { get; set; }

    // Navigation Properties
    public Process Process { get; set; } = null!;
    public ICollection<ProcessAction> Actions { get; set; } = new List<ProcessAction>();
}

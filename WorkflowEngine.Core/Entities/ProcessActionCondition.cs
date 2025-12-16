using System;
using WorkflowEngine.Core.Common;

namespace WorkflowEngine.Core.Entities;

public class ProcessActionCondition : BaseEntity
{
    public Guid ProcessActionId { get; set; }
    public Guid? TargetStepId { get; set; }
    public required string RuleExpression { get; set; }

    // Navigation Properties
    public ProcessAction ProcessAction { get; set; } = null!;
    public ProcessStep? TargetStep { get; set; }
}

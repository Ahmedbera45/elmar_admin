using System;
using System.Collections.Generic;
using WorkflowEngine.Core.Common;
using WorkflowEngine.Core.Enums;

namespace WorkflowEngine.Core.Entities;

public class ProcessAction : BaseEntity
{
    public Guid ProcessStepId { get; set; }
    public required string Name { get; set; }
    public ProcessActionType ActionType { get; set; }

    public bool IsCommentRequired { get; set; }
    public int? TimeoutSeconds { get; set; }
    public Guid? TimeoutActionId { get; set; }
    public Guid? DefaultConditionId { get; set; }
    public string? WebhookUrl { get; set; }
    public string? WebhookMethod { get; set; }

    /// <summary>
    /// The step to transition to when this action is executed.
    /// If null, it might keep the process in the same step.
    /// </summary>
    public Guid? TargetStepId { get; set; }

    // Navigation Properties
    public ProcessStep ProcessStep { get; set; } = null!;
    public ProcessStep? TargetStep { get; set; }
    public ProcessAction? TimeoutAction { get; set; }
    public ProcessActionCondition? DefaultCondition { get; set; }
    public ICollection<ProcessActionCondition> Conditions { get; set; } = new List<ProcessActionCondition>();
}

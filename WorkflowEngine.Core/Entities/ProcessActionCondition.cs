using System;
using WorkflowEngine.Core.Common;

namespace WorkflowEngine.Core.Entities;

public class ProcessActionCondition : BaseEntity
{
    public Guid ProcessActionId { get; set; }

    // Example: "Role", "Amount", "Department"
    public required string Key { get; set; }

    // Example: "Equals", "GreaterThan", "Contains"
    public required string Operator { get; set; }

    // Example: "Manager", "1000", "IT"
    public required string Value { get; set; }

    // Navigation Property
    public ProcessAction ProcessAction { get; set; } = null!;
}

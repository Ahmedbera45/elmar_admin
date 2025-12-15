using System.Collections.Generic;
using WorkflowEngine.Core.Common;

namespace WorkflowEngine.Core.Entities;

public class Process : BaseEntity
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation Property
    public ICollection<ProcessStep> Steps { get; set; } = new List<ProcessStep>();
}

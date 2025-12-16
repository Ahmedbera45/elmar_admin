using System;
using WorkflowEngine.Core.Common;
using WorkflowEngine.Core.Enums;

namespace WorkflowEngine.Core.Entities;

public class PePsConnection : BaseEntity
{
    public Guid ProcessStepId { get; set; }
    public Guid ProcessEntryId { get; set; }
    public int OrderIndex { get; set; }
    public ProcessEntryPermissionType PermissionType { get; set; }
    public string? VisibilityRule { get; set; }

    // Navigation Properties
    public ProcessStep ProcessStep { get; set; } = null!;
    public ProcessEntry ProcessEntry { get; set; } = null!;
}

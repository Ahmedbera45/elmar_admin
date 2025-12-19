using System;
using System.Collections.Generic;
using WorkflowEngine.Core.Common;
using WorkflowEngine.Core.Enums;

namespace WorkflowEngine.Core.Entities;

public class Process : BaseEntity
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public int Version { get; set; }
    public required string Code { get; set; }
    public Guid? ParentProcessId { get; set; }

    public string? AllowedRoles { get; set; }

    // Phase 11: Orchestration
    public string? PrerequisiteProcessCode { get; set; }
    public ProcessRequestStatus? PrerequisiteStatus { get; set; }
    public string? PrerequisiteKeyMap { get; set; }

    public Process? ParentProcess { get; set; }
    public ICollection<ProcessStep> Steps { get; set; } = new List<ProcessStep>();
}

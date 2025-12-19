using System;
using System.Collections.Generic;
using WorkflowEngine.Core.Enums;

namespace WorkflowEngine.Core.DTOs;

public class CreateProcessDto
{
    public required string Name { get; set; }
    public required string Code { get; set; }
}

public class CreateStepDto
{
    public Guid ProcessId { get; set; }
    public required string Name { get; set; }
    public ProcessStepType StepType { get; set; }
    public int OrderIndex { get; set; }
}

public class CreateActionDto
{
    public Guid StepId { get; set; }
    public required string Name { get; set; }
    public ProcessActionType ActionType { get; set; }
    public Guid? TargetStepId { get; set; }
}

public class CreateFieldDto
{
    public Guid StepId { get; set; }
    public required string Key { get; set; }
    public required string Title { get; set; }
    public ProcessEntryType EntryType { get; set; }
    public bool IsRequired { get; set; }
    public string? Options { get; set; }
    public string? LookupSource { get; set; }
}

public class UpdateStepDto
{
    public Guid StepId { get; set; }
    public ProcessStepAssignmentType AssignmentType { get; set; }
    public string? AssignedTo { get; set; }
}

public class ProcessDefinitionDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public List<StepDefinitionDto> Steps { get; set; } = new List<StepDefinitionDto>();
}

public class StepDefinitionDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ProcessStepType StepType { get; set; }
    public ProcessStepAssignmentType AssignmentType { get; set; }
    public string? AssignedTo { get; set; }
    public List<ActionDefinitionDto> Actions { get; set; } = new List<ActionDefinitionDto>();
    public List<FieldDefinitionDto> Fields { get; set; } = new List<FieldDefinitionDto>();
}

public class ActionDefinitionDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ProcessActionType ActionType { get; set; }
    public Guid? TargetStepId { get; set; }
}

public class FieldDefinitionDto
{
    public Guid Id { get; set; } // ProcessEntry Id
    public string Key { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public ProcessEntryType EntryType { get; set; }
    public bool IsRequired { get; set; }
}

public class WebUserDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

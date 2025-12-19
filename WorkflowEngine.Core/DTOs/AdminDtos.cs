using System;
using System.Collections.Generic;
using WorkflowEngine.Core.Enums;

namespace WorkflowEngine.Core.DTOs;

public class WebUserDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class ProcessDefinitionDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public int Version { get; set; }
    public bool IsActive { get; set; }
    public string? Description { get; set; }
    public List<StepDefinitionDto> Steps { get; set; } = new();
}

public class StepDefinitionDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ProcessStepType StepType { get; set; }
    public int OrderIndex { get; set; }
    public ProcessStepAssignmentType AssignmentType { get; set; }
    public string? AssignedTo { get; set; }
    public List<ActionDefinitionDto> Actions { get; set; } = new();
    public List<FieldDefinitionDto> Fields { get; set; } = new();
}

public class FieldDefinitionDto
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public ProcessEntryType EntryType { get; set; }
    public bool IsRequired { get; set; }
    public string? Options { get; set; }
    public string? LookupSource { get; set; }
}

public class ActionDefinitionDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ProcessActionType ActionType { get; set; }
    public Guid? TargetStepId { get; set; }
    public bool IsCommentRequired { get; set; }
    public string? RuleExpression { get; set; }
}

public class CreateProcessDto
{
    public required string Name { get; set; }
    public required string Code { get; set; }
}

public class CreateStepDto
{
    public Guid ProcessId { get; set; }
    public string Name { get; set; } = string.Empty;
    public ProcessStepType StepType { get; set; }
    public int OrderIndex { get; set; }
    public ProcessStepAssignmentType AssignmentType { get; set; }
    public string? AssignedTo { get; set; }
}

public class UpdateStepDto
{
    public Guid StepId { get; set; }
    public int AssignmentType { get; set; }
    public string? AssignedTo { get; set; }
}

public class CreateActionDto
{
    public Guid StepId { get; set; }
    public string Name { get; set; } = string.Empty;
    public ProcessActionType ActionType { get; set; }
    public Guid? TargetStepId { get; set; }
    public bool IsCommentRequired { get; set; }
    public string? WebhookUrl { get; set; }
    public string? WebhookMethod { get; set; }
    public string? RuleExpression { get; set; }
}

public class CreateFieldDto
{
    public Guid StepId { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public ProcessEntryType EntryType { get; set; }
    public bool IsRequired { get; set; }
    public string? Options { get; set; }
    public string? LookupSource { get; set; }
    public Guid? ExternalDatasetId { get; set; }
}

public class TemplateDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public Guid ProcessId { get; set; }
}

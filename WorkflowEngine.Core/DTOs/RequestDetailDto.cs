using System;
using System.Collections.Generic;
using WorkflowEngine.Core.Entities;
using WorkflowEngine.Core.Enums;

namespace WorkflowEngine.Core.DTOs;

public class RequestDetailDto
{
    public Guid Id { get; set; }
    public string RequestNumber { get; set; } = string.Empty;
    public string ProcessName { get; set; } = string.Empty;
    public string CurrentStepName { get; set; } = string.Empty;
    public ProcessRequestStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public string InitiatorName { get; set; } = string.Empty;

    public Dictionary<string, object?> FormValues { get; set; } = new Dictionary<string, object?>();
    public List<ProcessHistoryDto> History { get; set; } = new List<ProcessHistoryDto>();
    public List<ProcessActionDto> NextActions { get; set; } = new List<ProcessActionDto>();
}

public class ProcessActionDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ProcessActionType ActionType { get; set; }
    public bool IsCommentRequired { get; set; }
}

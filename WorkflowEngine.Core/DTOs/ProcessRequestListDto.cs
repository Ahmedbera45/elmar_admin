using System;
using System.Collections.Generic;
using WorkflowEngine.Core.Enums;

namespace WorkflowEngine.Core.DTOs;

public class ProcessRequestListDto
{
    public Guid Id { get; set; }
    public ProcessRequestStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid InitiatorUserId { get; set; }
    public Dictionary<string, object?> DynamicValues { get; set; } = new Dictionary<string, object?>();
}

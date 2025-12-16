using System;

namespace WorkflowEngine.Core.DTOs;

public class ProcessHistoryDto
{
    public required string ActionName { get; set; }
    public required string ActorName { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}

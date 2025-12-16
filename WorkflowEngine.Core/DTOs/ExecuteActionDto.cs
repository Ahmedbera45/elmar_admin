using System;
using System.Collections.Generic;

namespace WorkflowEngine.Core.DTOs;

public class ExecuteActionDto
{
    public Guid RequestId { get; set; }
    public Guid? ActionId { get; set; }
    public string? ActionName { get; set; }
    public Guid UserId { get; set; }
    public string? Comments { get; set; } // Added for Phase 6
    public Dictionary<string, object> FormValues { get; set; } = new();
}

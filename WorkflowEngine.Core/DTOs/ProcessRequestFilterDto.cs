using System;
using WorkflowEngine.Core.Enums;

namespace WorkflowEngine.Core.DTOs;

public class ProcessRequestFilterDto
{
    public string ProcessCode { get; set; } = string.Empty;
    public ProcessRequestStatus? Status { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

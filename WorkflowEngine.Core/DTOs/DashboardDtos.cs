using System.Collections.Generic;

namespace WorkflowEngine.Core.DTOs;

public class DashboardStatsDto
{
    public int Pending { get; set; }
    public int Approved { get; set; }
    public int Rejected { get; set; }
    public int Total { get; set; }
}

public class ChartDataDto
{
    public string Name { get; set; } = string.Empty;
    public int Count { get; set; }
}

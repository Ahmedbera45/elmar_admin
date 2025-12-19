using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WorkflowEngine.Core.DTOs;

namespace WorkflowEngine.Core.Interfaces;

public interface IDashboardService
{
    Task<DashboardStatsDto> GetStatsAsync(Guid userId, string role);
    Task<List<ChartDataDto>> GetChartDataAsync(Guid userId, string role);
}

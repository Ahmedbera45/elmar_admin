using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkflowEngine.Core.Interfaces;

namespace WorkflowEngine.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value);
        var role = User.FindFirst(ClaimTypes.Role)?.Value ?? "User";

        var stats = await _dashboardService.GetStatsAsync(userId, role);
        return Ok(stats);
    }

    [HttpGet("chart-data")]
    public async Task<IActionResult> GetChartData()
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value);
        var role = User.FindFirst(ClaimTypes.Role)?.Value ?? "User";

        var data = await _dashboardService.GetChartDataAsync(userId, role);
        return Ok(data);
    }
}

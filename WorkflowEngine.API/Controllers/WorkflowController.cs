using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkflowEngine.Core.DTOs;
using WorkflowEngine.Core.Interfaces;

namespace WorkflowEngine.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class WorkflowController : ControllerBase
{
    private readonly IWorkflowService _workflowService;

    public WorkflowController(IWorkflowService workflowService)
    {
        _workflowService = workflowService;
    }

    [HttpPost("start")]
    public async Task<IActionResult> StartProcess([FromQuery] string processCode)
    {
        var userIdClaim = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);
        if (userIdClaim == null) return Unauthorized();

        var userId = Guid.Parse(userIdClaim.Value);

        try
        {
            var requestId = await _workflowService.StartProcessAsync(processCode, userId);
            return Ok(new { RequestId = requestId });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("execute")]
    public async Task<IActionResult> ExecuteAction([FromBody] ExecuteActionDto dto)
    {
        var userIdClaim = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);
        if (userIdClaim == null) return Unauthorized();

        dto.UserId = Guid.Parse(userIdClaim.Value);

        try
        {
            await _workflowService.ExecuteActionAsync(dto);
            return Ok(new { Message = "Action executed successfully." });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("tasks")]
    public async Task<IActionResult> GetMyTasks()
    {
        var userIdClaim = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);
        if (userIdClaim == null) return Unauthorized();

        var userId = Guid.Parse(userIdClaim.Value);

        var tasks = await _workflowService.GetUserTasksAsync(userId);
        return Ok(tasks);
    }

    [HttpGet("history/{requestId}")]
    public async Task<IActionResult> GetHistory(Guid requestId)
    {
        try
        {
            var history = await _workflowService.GetRequestHistoryAsync(requestId);
            return Ok(history);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}

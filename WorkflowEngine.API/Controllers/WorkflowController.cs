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
        // Extract userId from Claims
        // var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        // For Phase 5 Simplicity, we assume it's passed or extracted.
        // Let's use a hardcoded user ID or extract properly if claim exists.
        // Reverting to extracting from Claim 'sub' which we set in JwtTokenGenerator

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

        // Ensure the DTO uses the authenticated user's ID for security
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
}

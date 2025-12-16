using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
    private readonly IStorageService _storageService;

    public WorkflowController(IWorkflowService workflowService, IStorageService storageService)
    {
        _workflowService = workflowService;
        _storageService = storageService;
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

    [HttpPost("upload")]
    public async Task<IActionResult> UploadFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");

        try
        {
            using var stream = file.OpenReadStream();
            var path = await _storageService.UploadAsync(stream, file.FileName);
            return Ok(new { FilePath = path });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("download/{*fileName}")] // *fileName allows slashes in path if encoded or handled by routing, though usually safer to use query param or replace slashes.
    // The requirement says "download/{fileName}". Since I return relative path "Year/Month/Guid.ext", I should probably accept that.
    // Routing might be tricky with slashes. Let's try [FromQuery] or catch-all.
    // Catch-all route parameter is safer for paths.
    public async Task<IActionResult> DownloadFile(string fileName)
    {
        try
        {
            var (stream, contentType, originalName) = await _storageService.DownloadAsync(fileName);
            return File(stream, contentType, originalName);
        }
        catch (FileNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}

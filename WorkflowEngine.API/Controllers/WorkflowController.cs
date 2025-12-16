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

    /// <summary>
    /// Starts a new process instance.
    /// </summary>
    /// <param name="processCode">The unique code of the process to start.</param>
    /// <returns>The ID of the created process request.</returns>
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

    /// <summary>
    /// Executes an action on a process request to transition to the next step.
    /// </summary>
    /// <param name="dto">The execution details including RequestId, ActionName, and FormValues.</param>
    /// <returns>Success message.</returns>
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

    /// <summary>
    /// Retrieves active tasks for the current user.
    /// </summary>
    /// <returns>A list of active process requests.</returns>
    [HttpGet("tasks")]
    public async Task<IActionResult> GetMyTasks()
    {
        var userIdClaim = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);
        if (userIdClaim == null) return Unauthorized();

        var userId = Guid.Parse(userIdClaim.Value);

        var tasks = await _workflowService.GetUserTasksAsync(userId);
        return Ok(tasks);
    }

    /// <summary>
    /// Retrieves the history/audit trail of a specific process request.
    /// </summary>
    /// <param name="requestId">The ID of the process request.</param>
    /// <returns>A list of history records.</returns>
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

    /// <summary>
    /// Uploads a file to the storage system.
    /// </summary>
    /// <param name="file">The file to upload.</param>
    /// <returns>The stored file path.</returns>
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

    /// <summary>
    /// Downloads a file from the storage system.
    /// </summary>
    /// <param name="fileName">The relative path/filename of the stored file.</param>
    /// <returns>The file stream.</returns>
    [HttpGet("download/{*fileName}")]
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

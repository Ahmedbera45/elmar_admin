using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkflowEngine.Core.DTOs;
using WorkflowEngine.Core.Interfaces;

namespace WorkflowEngine.API.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;
    private readonly Infrastructure.Services.ProcessImportExportService _importExportService;
    private readonly Infrastructure.Services.ProcessVersioningService _versioningService;
    private readonly Infrastructure.Data.AppDbContext _context; // For Versions list query (quick fix)

    public AdminController(
        IAdminService adminService,
        Infrastructure.Services.ProcessImportExportService importExportService,
        Infrastructure.Services.ProcessVersioningService versioningService,
        Infrastructure.Data.AppDbContext context)
    {
        _adminService = adminService;
        _importExportService = importExportService;
        _versioningService = versioningService;
        _context = context;
    }

    [HttpGet("process/{id}/export")]
    public async Task<IActionResult> ExportProcess(Guid id)
    {
        var json = await _importExportService.ExportProcessAsync(id);
        return File(System.Text.Encoding.UTF8.GetBytes(json), "application/json", $"process-{id}.json");
    }

    [HttpPost("process/import")]
    public async Task<IActionResult> ImportProcess(Microsoft.AspNetCore.Http.IFormFile file)
    {
        if (file == null || file.Length == 0) return BadRequest("No file");

        using var stream = new System.IO.StreamReader(file.OpenReadStream());
        var json = await stream.ReadToEndAsync();

        try {
            await _importExportService.ImportProcessAsync(json);
            return Ok("Import successful");
        } catch (Exception ex) {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("process/{id}/new-version")]
    public async Task<IActionResult> NewVersion(Guid id)
    {
        var newId = await _versioningService.CreateNewVersionAsync(id);
        return Ok(new { Id = newId });
    }

    [HttpGet("process/{code}/versions")]
    public async Task<IActionResult> GetVersions(string code)
    {
        // Should move to Service but for speed:
        var versions = await _context.Processes
            .Where(p => p.Code == code)
            .OrderByDescending(p => p.Version)
            .Select(p => new { p.Id, p.Version, p.IsActive, p.CreatedAt })
            .ToListAsync();
        return Ok(versions);
    }

    [HttpPost("process/{id}/restore")]
    public async Task<IActionResult> RestoreVersion(Guid id)
    {
        var process = await _context.Processes.FindAsync(id);
        if (process == null) return NotFound();

        // Deactivate all others with same code
        var others = await _context.Processes
            .Where(p => p.Code == process.Code)
            .ToListAsync();

        foreach(var p in others) p.IsActive = false;

        process.IsActive = true;
        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpPost("process")]
    public async Task<IActionResult> CreateProcess([FromBody] CreateProcessDto dto)
    {
        var id = await _adminService.CreateProcessAsync(dto);
        return Ok(new { Id = id });
    }

    [HttpPost("step")]
    public async Task<IActionResult> AddStep([FromBody] CreateStepDto dto)
    {
        var id = await _adminService.AddStepAsync(dto);
        return Ok(new { Id = id });
    }

    [HttpPut("step")]
    public async Task<IActionResult> UpdateStep([FromBody] UpdateStepDto dto)
    {
        await _adminService.UpdateStepAsync(dto);
        return Ok();
    }

    [HttpPost("action")]
    public async Task<IActionResult> AddAction([FromBody] CreateActionDto dto)
    {
        var id = await _adminService.AddActionAsync(dto);
        return Ok(new { Id = id });
    }

    [HttpPost("field")]
    public async Task<IActionResult> AddField([FromBody] CreateFieldDto dto)
    {
        var id = await _adminService.AddFieldAsync(dto);
        return Ok(new { Id = id });
    }

    [HttpGet("process/{id}")]
    public async Task<IActionResult> GetProcessDefinition(Guid id)
    {
        var def = await _adminService.GetProcessDefinitionAsync(id);
        if (def == null) return NotFound();
        return Ok(def);
    }

    [HttpGet("processes")]
    public async Task<IActionResult> GetAllProcesses()
    {
        var list = await _adminService.GetAllProcessesAsync();
        return Ok(list);
    }

    [HttpGet("templates")]
    public async Task<IActionResult> GetTemplates()
    {
        var list = await _adminService.GetTemplatesAsync();
        return Ok(list);
    }

    [HttpGet("templates/{id}")]
    public async Task<IActionResult> GetTemplate(Guid id)
    {
        var t = await _adminService.GetTemplateAsync(id);
        if (t == null) return NotFound();
        return Ok(t);
    }

    [HttpPost("templates")]
    public async Task<IActionResult> SaveTemplate([FromBody] TemplateDto dto)
    {
        var id = await _adminService.SaveTemplateAsync(dto);
        return Ok(new { Id = id });
    }
}

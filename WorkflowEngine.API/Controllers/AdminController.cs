using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkflowEngine.Core.DTOs;
using WorkflowEngine.Core.Interfaces;

namespace WorkflowEngine.API.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
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

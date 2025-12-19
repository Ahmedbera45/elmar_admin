using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkflowEngine.Core.Entities;
using WorkflowEngine.Core.Interfaces;
using WorkflowEngine.Infrastructure.Data;

namespace WorkflowEngine.API.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/[controller]")]
public class IntegrationController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IExternalDataService _dataService;

    public IntegrationController(AppDbContext context, IExternalDataService dataService)
    {
        _context = context;
        _dataService = dataService;
    }

    // Connections
    [HttpGet("connections")]
    public async Task<IActionResult> GetConnections()
    {
        return Ok(await _context.ExternalConnections.ToListAsync());
    }

    [HttpPost("connections")]
    public async Task<IActionResult> CreateConnection([FromBody] ExternalConnection conn)
    {
        conn.Id = Guid.NewGuid();
        conn.CreatedAt = DateTime.UtcNow;
        _context.ExternalConnections.Add(conn);
        await _context.SaveChangesAsync();
        return Ok(conn);
    }

    [HttpPost("connections/{id}/test")]
    public async Task<IActionResult> TestConnection(Guid id)
    {
        var result = await _dataService.TestConnectionAsync(id);
        return Ok(new { Success = result });
    }

    // Datasets
    [HttpGet("datasets")]
    public async Task<IActionResult> GetDatasets()
    {
        return Ok(await _context.ExternalDatasets.Include(d => d.ExternalConnection).ToListAsync());
    }

    [HttpPost("datasets")]
    public async Task<IActionResult> CreateDataset([FromBody] ExternalDataset ds)
    {
        ds.Id = Guid.NewGuid();
        ds.CreatedAt = DateTime.UtcNow;
        _context.ExternalDatasets.Add(ds);
        await _context.SaveChangesAsync();
        return Ok(ds);
    }

    [HttpPost("datasets/{id}/preview")]
    public async Task<IActionResult> PreviewDataset(Guid id, [FromBody] Dictionary<string, object> paramsDict)
    {
        try {
            var data = await _dataService.ExecuteDatasetAsync(id, paramsDict ?? new());
            return Ok(data);
        } catch (Exception ex) {
            return BadRequest(ex.Message);
        }
    }

    // Runtime Execution (Allow Users)
    [Authorize]
    [HttpPost("execute/{datasetId}")]
    public async Task<IActionResult> ExecuteDataset(Guid datasetId, [FromBody] Dictionary<string, object> paramsDict)
    {
        // For security, we might want to restrict which datasets user can access,
        // but typically if it's used in a form, they have read access.
        try {
            var data = await _dataService.ExecuteDatasetAsync(datasetId, paramsDict ?? new());
            return Ok(data);
        } catch (Exception ex) {
            return BadRequest(ex.Message);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkflowEngine.Core.Entities;
using WorkflowEngine.Infrastructure.Data;

namespace WorkflowEngine.API.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/audit")]
public class AuditController : ControllerBase
{
    private readonly AppDbContext _context;

    public AuditController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("logs")]
    public async Task<IActionResult> GetAuditLogs([FromQuery] Guid? entityId, [FromQuery] Guid? userId, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        var query = _context.AuditLogs.AsQueryable();

        if (entityId.HasValue)
        {
            // Search in RecordId (which is stringified JSON key, or just ID?)
            // Implementation in AppDbContext uses simple ID for single PK or JSON for composite.
            // Usually BaseEntity uses Guid Id.
            // Let's assume most RecordId are just the Guid string or contain it.
            // The JSON format in AppDbContext: `JsonSerializer.Serialize(KeyValues)`
            // e.g. {"Id":"..."}
            string search = entityId.Value.ToString();
            query = query.Where(a => a.RecordId.Contains(search));
        }

        if (userId.HasValue)
        {
            query = query.Where(a => a.UserId == userId.Value.ToString());
        }

        if (startDate.HasValue)
            query = query.Where(a => a.Timestamp >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(a => a.Timestamp <= endDate.Value);

        var logs = await query.OrderByDescending(a => a.Timestamp).Take(100).ToListAsync();

        return Ok(logs);
    }
}

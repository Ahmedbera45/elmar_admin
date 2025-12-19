using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkflowEngine.Core.Entities;
using WorkflowEngine.Infrastructure.Data;

namespace WorkflowEngine.API.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/admin/notifications")]
public class NotificationsController : ControllerBase
{
    private readonly AppDbContext _context;

    public NotificationsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("templates")]
    public async Task<IActionResult> GetTemplates()
    {
        var templates = await _context.NotificationTemplates.ToListAsync();
        return Ok(templates);
    }

    [HttpPost("templates")]
    public async Task<IActionResult> SaveTemplate([FromBody] NotificationTemplate template)
    {
        var existing = await _context.NotificationTemplates.FirstOrDefaultAsync(t => t.Key == template.Key);
        if (existing == null)
        {
            template.Id = Guid.NewGuid();
            template.CreatedAt = DateTime.UtcNow;
            _context.NotificationTemplates.Add(template);
        }
        else
        {
            existing.SubjectTemplate = template.SubjectTemplate;
            existing.BodyTemplate = template.BodyTemplate;
            existing.Description = template.Description;
            existing.ModifiedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return Ok(existing ?? template);
    }
}

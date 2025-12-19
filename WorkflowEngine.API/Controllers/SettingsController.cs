using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkflowEngine.Core.Entities;
using WorkflowEngine.Infrastructure.Data;

namespace WorkflowEngine.API.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/admin/settings")]
public class SettingsController : ControllerBase
{
    private readonly AppDbContext _context;

    public SettingsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetSettings()
    {
        var settings = await _context.SystemSettings.ToListAsync();
        return Ok(settings);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateSetting([FromBody] SystemSetting setting)
    {
        var existing = await _context.SystemSettings.FirstOrDefaultAsync(s => s.Key == setting.Key);
        if (existing == null)
        {
            setting.Id = Guid.NewGuid();
            setting.CreatedAt = DateTime.UtcNow;
            _context.SystemSettings.Add(setting);
        }
        else
        {
            existing.Value = setting.Value;
            existing.Description = setting.Description;
            existing.ModifiedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return Ok(existing ?? setting);
    }
}

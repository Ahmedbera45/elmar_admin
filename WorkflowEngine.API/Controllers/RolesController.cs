using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkflowEngine.Core.Entities;
using WorkflowEngine.Infrastructure.Data;

namespace WorkflowEngine.API.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/admin/roles")]
public class RolesController : ControllerBase
{
    private readonly AppDbContext _context;

    public RolesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetRoles()
    {
        var roles = await _context.AppRoles.ToListAsync();
        return Ok(roles);
    }

    [HttpPost]
    public async Task<IActionResult> CreateRole([FromBody] AppRole role)
    {
        if (await _context.AppRoles.AnyAsync(r => r.Name == role.Name))
            return BadRequest("Role already exists.");

        role.Id = Guid.NewGuid();
        role.CreatedAt = DateTime.UtcNow;
        _context.AppRoles.Add(role);
        await _context.SaveChangesAsync();
        return Ok(role);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRole(Guid id)
    {
        var role = await _context.AppRoles.FindAsync(id);
        if (role == null) return NotFound();

        _context.AppRoles.Remove(role);
        await _context.SaveChangesAsync();
        return Ok();
    }
}

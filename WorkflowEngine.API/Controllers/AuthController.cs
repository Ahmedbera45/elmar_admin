using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkflowEngine.Core.DTOs;
using WorkflowEngine.Core.Entities;
using WorkflowEngine.Core.Interfaces;
using WorkflowEngine.Infrastructure.Data;

namespace WorkflowEngine.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public AuthController(AppDbContext context, IJwtTokenGenerator jwtTokenGenerator)
    {
        _context = context;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _context.WebUsers
            .FirstOrDefaultAsync(u => u.Username == request.Username);

        if (user == null || !user.IsActive)
        {
            return Unauthorized("Invalid credentials or inactive user.");
        }

        // Simple string comparison for now as requested (TODO: BCrypt)
        if (user.PasswordHash != request.Password)
        {
            return Unauthorized("Invalid credentials.");
        }

        var token = _jwtTokenGenerator.GenerateToken(user);
        return Ok(new { Token = token });
    }
}

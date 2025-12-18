using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
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

    /// <summary>
    /// Authenticates a user and returns a JWT token.
    /// </summary>
    /// <param name="request">Login credentials (username/password)</param>
    /// <returns>JWT Token and Refresh Token</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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

        var authResult = _jwtTokenGenerator.GenerateToken(user);
        return Ok(authResult);
    }

    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(AuthResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var principal = _jwtTokenGenerator.GetPrincipalFromToken(request.Token);
        if (principal == null)
        {
            return BadRequest("Invalid Token");
        }

        // var expiryDateUnix = long.Parse(principal.Claims.Single(x => x.Type == System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Exp).Value);
        // var expiryDateTimeUtc = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(expiryDateUnix);

        var jti = principal.Claims.SingleOrDefault(x => x.Type == System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Jti)?.Value;

        var storedRefreshToken = await _context.RefreshTokens
            .Include(x => x.User)
            .SingleOrDefaultAsync(x => x.Token == request.RefreshToken);

        if (storedRefreshToken == null)
        {
            return BadRequest("Refresh Token does not exist");
        }

        if (DateTime.UtcNow > storedRefreshToken.ExpiryDate)
        {
            return BadRequest("Refresh Token has expired");
        }

        if (storedRefreshToken.Invalidated)
        {
            return BadRequest("Refresh Token has been invalidated");
        }

        if (storedRefreshToken.Used)
        {
            return BadRequest("Refresh Token has been used");
        }

        // Mark as used
        storedRefreshToken.Used = true;
        _context.RefreshTokens.Update(storedRefreshToken);
        await _context.SaveChangesAsync();

        var user = storedRefreshToken.User;
        var authResult = _jwtTokenGenerator.GenerateToken(user);

        return Ok(authResult);
    }
}

using System;
using System.Security.Claims;
using WorkflowEngine.Core.DTOs;
using WorkflowEngine.Core.Entities;

namespace WorkflowEngine.Core.Interfaces;

public interface IJwtTokenGenerator
{
    AuthResult GenerateToken(WebUser user);
    RefreshToken GenerateRefreshToken(WebUser user);
    ClaimsPrincipal? GetPrincipalFromToken(string token);
}

using System;
using WorkflowEngine.Core.Entities;

namespace WorkflowEngine.Core.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(WebUser user);
}

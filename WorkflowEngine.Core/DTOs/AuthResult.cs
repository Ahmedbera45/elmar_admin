using System.Collections.Generic;

namespace WorkflowEngine.Core.DTOs;

public class AuthResult
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public bool Success { get; set; }
    public List<string> Errors { get; set; } = new List<string>();
}

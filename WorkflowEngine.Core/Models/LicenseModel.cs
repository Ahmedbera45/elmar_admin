using System;

namespace WorkflowEngine.Core.Models;

public class LicenseModel
{
    public required string MachineHash { get; set; }
    public DateTime ExpirationDate { get; set; }
    public bool IsActive { get; set; }
}

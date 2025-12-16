using System;
using WorkflowEngine.Core.Interfaces;
using WorkflowEngine.Core.Models;

namespace WorkflowEngine.Infrastructure.Security;

public class LicenseValidator : ILicenseValidator
{
    private readonly IMachineIdGenerator _machineIdGenerator;

    public LicenseValidator(IMachineIdGenerator machineIdGenerator)
    {
        _machineIdGenerator = machineIdGenerator;
    }

    public bool Validate()
    {
        var currentMachineId = _machineIdGenerator.GetMachineId();

        // MOCK: In a real scenario, this would load from a file (License.json) or encrypted string.
        // For development/mocking purposes, we'll create a valid license for the current machine.
        var mockLicense = new LicenseModel
        {
            MachineHash = currentMachineId, // Matches current machine so it passes
            ExpirationDate = DateTime.UtcNow.AddYears(1), // Valid for 1 year
            IsActive = true
        };

        // Verification Logic
        if (!mockLicense.IsActive)
            return false;

        if (DateTime.UtcNow > mockLicense.ExpirationDate)
            return false;

        if (!string.Equals(currentMachineId, mockLicense.MachineHash, StringComparison.OrdinalIgnoreCase))
            return false;

        return true;
    }
}

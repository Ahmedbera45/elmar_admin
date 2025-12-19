using System;
using System.Threading.Tasks;
using WorkflowEngine.Core.Interfaces;

namespace WorkflowEngine.Infrastructure.Services;

public class MockDebtProvider : IDebtProvider
{
    public Task<string> CreateDebtAsync(Guid userId, decimal amount, string code, string description)
    {
        // Simulate debt creation
        Console.WriteLine($"[MockDebt] Created Debt: User={userId}, Amount={amount}, Code={code}");
        return Task.FromResult($"DEBT-{code}-{Guid.NewGuid().ToString().Substring(0,8)}");
    }
}

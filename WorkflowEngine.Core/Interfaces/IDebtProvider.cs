using System;
using System.Threading.Tasks;

namespace WorkflowEngine.Core.Interfaces;

public interface IDebtProvider
{
    Task<string> CreateDebtAsync(Guid userId, decimal amount, string code, string description);
}

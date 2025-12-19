using System;
using System.Threading.Tasks;
using WorkflowEngine.Core.Interfaces;

namespace WorkflowEngine.Infrastructure.Services;

public class MockPaymentProvider : IPaymentProvider
{
    public Task<string> ProcessPaymentAsync(decimal amount, string currency, string cardToken)
    {
        // Simulate payment
        if (cardToken == "FAIL") throw new Exception("Payment Failed");
        return Task.FromResult($"TX-{Guid.NewGuid().ToString().Substring(0,8).ToUpper()}");
    }

    public Task<bool> VerifyPaymentAsync(string transactionId)
    {
        return Task.FromResult(!string.IsNullOrEmpty(transactionId));
    }
}

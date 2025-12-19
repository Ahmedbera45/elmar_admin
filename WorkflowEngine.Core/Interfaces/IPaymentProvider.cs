using System.Threading.Tasks;

namespace WorkflowEngine.Core.Interfaces;

public interface IPaymentProvider
{
    Task<string> ProcessPaymentAsync(decimal amount, string currency, string cardToken);
    Task<bool> VerifyPaymentAsync(string transactionId);
}

using System;
using System.Threading.Tasks;
using WorkflowEngine.Core.Interfaces;

namespace WorkflowEngine.Infrastructure.Services;

public class MockSignatureProvider : ISignatureProvider
{
    // Fix: Parameter order matches Interface (Guid, string, Guid)
    public Task<string> SignDocumentAsync(Guid documentId, string content, Guid userId)
    {
        // Simulate signing process
        return Task.FromResult($"SIGN-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}");
    }

    public Task<bool> VerifySignatureAsync(string signedContent, string signature)
    {
        return Task.FromResult(!string.IsNullOrEmpty(signature));
    }
}

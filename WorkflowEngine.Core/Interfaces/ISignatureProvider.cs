using System;
using System.Threading.Tasks;

namespace WorkflowEngine.Core.Interfaces;

public interface ISignatureProvider
{
    Task<string> SignDocumentAsync(Guid documentId, string content, Guid userId);
    Task<bool> VerifySignatureAsync(string signedContent, string signature);
}

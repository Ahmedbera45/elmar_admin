using System;
using System.Threading.Tasks;

namespace WorkflowEngine.Core.Interfaces;

public interface IDocumentService
{
    Task<string> GenerateDocumentAsync(Guid templateId, Guid requestId);
}

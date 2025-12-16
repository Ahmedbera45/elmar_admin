using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WorkflowEngine.Core.Entities;

namespace WorkflowEngine.Core.Interfaces;

public interface IWorkflowService
{
    Task<Guid> StartProcessAsync(string processCode, Guid userId);
    Task ExecuteActionAsync(Guid entryId, string actionName, Guid userId, Dictionary<string, object> inputs);
    Task<List<ProcessRequest>> GetUserTasksAsync(Guid userId);
}

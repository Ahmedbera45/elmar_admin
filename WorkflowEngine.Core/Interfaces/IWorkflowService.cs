using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WorkflowEngine.Core.DTOs;
using WorkflowEngine.Core.Entities;

namespace WorkflowEngine.Core.Interfaces;

public interface IWorkflowService
{
    Task<Guid> StartProcessAsync(string processCode, Guid userId);
    Task ExecuteActionAsync(ExecuteActionDto dto);
    Task<List<ProcessRequest>> GetUserTasksAsync(Guid userId);
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WorkflowEngine.Core.DTOs;

namespace WorkflowEngine.Core.Interfaces;

public interface IAdminService
{
    Task<Guid> CreateProcessAsync(CreateProcessDto dto);
    Task<Guid> AddStepAsync(CreateStepDto dto);
    Task<Guid> AddActionAsync(CreateActionDto dto);
    Task<Guid> AddFieldAsync(CreateFieldDto dto);
    Task<ProcessDefinitionDto?> GetProcessDefinitionAsync(Guid processId);
    Task<List<ProcessDefinitionDto>> GetAllProcessesAsync();
}

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
    Task UpdateStepAsync(UpdateStepDto dto);
    Task<ProcessDefinitionDto?> GetProcessDefinitionAsync(Guid processId);
    Task<List<ProcessDefinitionDto>> GetAllProcessesAsync();
    Task<List<TemplateDto>> GetTemplatesAsync();
    Task<TemplateDto?> GetTemplateAsync(Guid id);
    Task<Guid> SaveTemplateAsync(TemplateDto dto);
}

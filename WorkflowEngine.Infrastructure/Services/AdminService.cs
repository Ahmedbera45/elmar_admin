using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WorkflowEngine.Core.DTOs;
using WorkflowEngine.Core.Entities;
using WorkflowEngine.Core.Interfaces;
using WorkflowEngine.Infrastructure.Data;

namespace WorkflowEngine.Infrastructure.Services;

public class AdminService : IAdminService
{
    private readonly AppDbContext _context;
    private readonly ILogger<AdminService> _logger;

    public AdminService(AppDbContext context, ILogger<AdminService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Guid> CreateProcessAsync(CreateProcessDto dto)
    {
        var process = new Process
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Code = dto.Code,
            Version = 1,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        _context.Processes.Add(process);
        await _context.SaveChangesAsync();
        return process.Id;
    }

    public async Task<Guid> AddStepAsync(CreateStepDto dto)
    {
        var step = new ProcessStep
        {
            Id = Guid.NewGuid(),
            ProcessId = dto.ProcessId,
            Name = dto.Name,
            StepType = dto.StepType,
            OrderIndex = dto.OrderIndex,
            CreatedAt = DateTime.UtcNow
        };
        _context.ProcessSteps.Add(step);
        await _context.SaveChangesAsync();
        return step.Id;
    }

    public async Task<Guid> AddActionAsync(CreateActionDto dto)
    {
        var action = new ProcessAction
        {
            Id = Guid.NewGuid(),
            ProcessStepId = dto.StepId,
            Name = dto.Name,
            ActionType = dto.ActionType,
            TargetStepId = dto.TargetStepId,
            CreatedAt = DateTime.UtcNow
        };
        _context.ProcessActions.Add(action);
        await _context.SaveChangesAsync();
        return action.Id;
    }

    public async Task<Guid> AddFieldAsync(CreateFieldDto dto)
    {
        var entry = new ProcessEntry
        {
            Id = Guid.NewGuid(),
            Key = dto.Key,
            Title = dto.Title,
            EntryType = dto.EntryType,
            IsRequired = dto.IsRequired,
            Options = dto.Options,
            CreatedAt = DateTime.UtcNow
        };
        _context.ProcessEntries.Add(entry);

        var connection = new PePsConnection
        {
            Id = Guid.NewGuid(),
            ProcessStepId = dto.StepId,
            ProcessEntryId = entry.Id,
            OrderIndex = 0,
            PermissionType = WorkflowEngine.Core.Enums.ProcessEntryPermissionType.Write,
            CreatedAt = DateTime.UtcNow
        };
        _context.PePsConnections.Add(connection);

        await _context.SaveChangesAsync();
        return entry.Id;
    }

    public async Task<ProcessDefinitionDto?> GetProcessDefinitionAsync(Guid processId)
    {
        var process = await _context.Processes
            .Include(p => p.Steps)
            .ThenInclude(s => s.Actions)
            .FirstOrDefaultAsync(p => p.Id == processId);

        if (process == null) return null;

        var dto = new ProcessDefinitionDto
        {
            Id = process.Id,
            Name = process.Name,
            Code = process.Code
        };

        var stepIds = process.Steps.Select(s => s.Id).ToList();
        var connections = await _context.PePsConnections
            .Include(c => c.ProcessEntry)
            .Where(c => stepIds.Contains(c.ProcessStepId))
            .ToListAsync();

        foreach (var step in process.Steps.OrderBy(s => s.OrderIndex))
        {
            var stepDto = new StepDefinitionDto
            {
                Id = step.Id,
                Name = step.Name,
                StepType = step.StepType,
                Actions = step.Actions.Select(a => new ActionDefinitionDto
                {
                    Id = a.Id,
                    Name = a.Name,
                    ActionType = a.ActionType,
                    TargetStepId = a.TargetStepId
                }).ToList()
            };

            var stepConns = connections.Where(c => c.ProcessStepId == step.Id).ToList();
            stepDto.Fields = stepConns.Select(c => new FieldDefinitionDto
            {
                Id = c.ProcessEntry.Id,
                Key = c.ProcessEntry.Key,
                Title = c.ProcessEntry.Title,
                EntryType = c.ProcessEntry.EntryType,
                IsRequired = c.ProcessEntry.IsRequired
            }).ToList();

            dto.Steps.Add(stepDto);
        }

        return dto;
    }

    public async Task<List<ProcessDefinitionDto>> GetAllProcessesAsync()
    {
        return await _context.Processes
            .Select(p => new ProcessDefinitionDto
            {
                Id = p.Id,
                Name = p.Name,
                Code = p.Code
            })
            .ToListAsync();
    }
}

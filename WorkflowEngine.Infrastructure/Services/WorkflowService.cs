using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WorkflowEngine.Core.DTOs;
using WorkflowEngine.Core.Entities;
using WorkflowEngine.Core.Enums;
using WorkflowEngine.Core.Interfaces;
using WorkflowEngine.Infrastructure.Data;

namespace WorkflowEngine.Infrastructure.Services;

public class WorkflowService : IWorkflowService
{
    private readonly AppDbContext _context;

    public WorkflowService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> StartProcessAsync(string processCode, Guid userId)
    {
        var process = await _context.Processes
            .FirstOrDefaultAsync(p => p.Code == processCode && p.IsActive);

        if (process == null)
        {
            throw new Exception($"Process not found or inactive: {processCode}");
        }

        var startStep = await _context.ProcessSteps
            .FirstOrDefaultAsync(s => s.ProcessId == process.Id && s.StepType == ProcessStepType.Start);

        if (startStep == null)
        {
            throw new Exception($"Start step not found for process: {processCode}");
        }

        var requestNumber = $"PR-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";

        var request = new ProcessRequest
        {
            ProcessId = process.Id,
            CurrentStepId = startStep.Id,
            Status = ProcessRequestStatus.Active,
            InitiatorUserId = userId,
            RequestNumber = requestNumber,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId.ToString()
        };

        _context.ProcessRequests.Add(request);

        var history = new ProcessRequestHistory
        {
            ProcessRequest = request,
            ToStepId = startStep.Id,
            ActorUserId = userId,
            ActionTime = DateTime.UtcNow,
            Comments = "Süreç Başlatıldı",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId.ToString()
        };

        _context.ProcessRequestHistories.Add(history);

        await _context.SaveChangesAsync();

        return request.Id;
    }

    public async Task ExecuteActionAsync(ExecuteActionDto dto)
    {
        // 1. Get Request
        var request = await _context.ProcessRequests
            .Include(e => e.CurrentStep)
            .ThenInclude(s => s.Actions)
            .ThenInclude(a => a.Conditions) // Include conditions for Rule Engine
            .FirstOrDefaultAsync(e => e.Id == dto.RequestId);

        if (request == null)
        {
            throw new Exception($"Process Request not found: {dto.RequestId}");
        }

        if (request.Status != ProcessRequestStatus.Active)
        {
            throw new Exception($"Process Request is not active. Status: {request.Status}");
        }

        // 2. Validate User Role (Mock: Check existence)
        // In real app, check PePsConnection permissions or Step roles
        var user = await _context.WebUsers.FindAsync(dto.UserId);
        if (user == null || !user.IsActive)
        {
            throw new UnauthorizedAccessException("User not authorized or inactive.");
        }

        // 3. Find the Action
        ProcessAction? action = null;
        if (dto.ActionId.HasValue)
        {
            action = request.CurrentStep.Actions.FirstOrDefault(a => a.Id == dto.ActionId.Value);
        }
        else if (!string.IsNullOrEmpty(dto.ActionName))
        {
            action = request.CurrentStep.Actions.FirstOrDefault(a => a.Name.Equals(dto.ActionName, StringComparison.OrdinalIgnoreCase));
        }

        if (action == null)
        {
            throw new Exception($"Action not available in step '{request.CurrentStep.Name}'");
        }

        // 4. Form Validation & Data Saving
        var stepConnections = await _context.PePsConnections
            .Include(c => c.ProcessEntry)
            .Where(c => c.ProcessStepId == request.CurrentStepId)
            .ToListAsync();

        foreach (var connection in stepConnections)
        {
            // Validate Required
            if (connection.ProcessEntry.IsRequired && connection.PermissionType == ProcessEntryPermissionType.Write)
            {
                if (!dto.FormValues.ContainsKey(connection.ProcessEntry.Key))
                {
                    throw new Exception($"Missing required field: {connection.ProcessEntry.Title} ({connection.ProcessEntry.Key})");
                }
            }

            // Save Value if Present
            if (dto.FormValues.TryGetValue(connection.ProcessEntry.Key, out var val) && val != null)
            {
                var entryValue = new ProcessRequestValue
                {
                    ProcessRequestId = request.Id,
                    ProcessEntryId = connection.ProcessEntry.Id,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = dto.UserId.ToString()
                };

                // Map value based on type
                switch (connection.ProcessEntry.EntryType)
                {
                    case ProcessEntryType.Text:
                    case ProcessEntryType.Select: // Assume select value is string
                    case ProcessEntryType.File:
                        entryValue.StringValue = val.ToString();
                        break;
                    case ProcessEntryType.Number:
                        if (int.TryParse(val.ToString(), out int iVal)) entryValue.IntValue = iVal;
                        else if (decimal.TryParse(val.ToString(), out decimal dVal)) entryValue.DecimalValue = dVal;
                        break;
                    case ProcessEntryType.Date:
                        if (DateTime.TryParse(val.ToString(), out DateTime dtVal)) entryValue.DateValue = dtVal;
                        break;
                    case ProcessEntryType.Checkbox:
                        if (bool.TryParse(val.ToString(), out bool bVal)) entryValue.BoolValue = bVal;
                        break;
                }

                _context.ProcessRequestValues.Add(entryValue);
            }
        }

        // 5. Rule Engine (Simple)
        Guid? targetStepId = action.TargetStepId;
        Guid? defaultConditionId = action.DefaultConditionId;

        // Check explicit conditions
        foreach (var condition in action.Conditions)
        {
            // Evaluate RuleExpression. Assuming "Key Operator Value" e.g., "amount > 1000"
            // For Phase 5, we do a very simple check or match against form values just submitted or saved.
            // Simplified: If RuleExpression matches "Key == Value" (exact string match logic for simplicity)
            // A real engine uses Expression Trees or a library like NRules/DynamicExpresso.

            // NOTE: Implementing full parser is out of scope, using placeholder logic:
            // If condition.RuleExpression is "true", we take it.
            if (condition.RuleExpression == "true") // Mock
            {
                if (condition.TargetStepId.HasValue)
                {
                    targetStepId = condition.TargetStepId.Value;
                    break; // First match wins
                }
            }
        }

        // 6. Update Request (Transition)
        var previousStepId = request.CurrentStepId;

        if (targetStepId.HasValue)
        {
            request.CurrentStepId = targetStepId.Value;

            var targetStep = await _context.ProcessSteps.FindAsync(targetStepId.Value);
            if (targetStep != null && targetStep.StepType == ProcessStepType.End)
            {
                request.Status = ProcessRequestStatus.Completed;
            }
        }

        // 7. History
        var history = new ProcessRequestHistory
        {
            ProcessRequestId = request.Id,
            FromStepId = previousStepId,
            ToStepId = targetStepId ?? previousStepId,
            ActionId = action.Id,
            ActorUserId = dto.UserId,
            ActionTime = DateTime.UtcNow,
            Comments = $"Executed action: {action.Name}",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = dto.UserId.ToString()
        };

        _context.ProcessRequestHistories.Add(history);

        await _context.SaveChangesAsync();
    }

    public async Task<List<ProcessRequest>> GetUserTasksAsync(Guid userId)
    {
        return await _context.ProcessRequests
            .Include(e => e.Process)
            .Include(e => e.CurrentStep)
            .Where(e => e.Status == ProcessRequestStatus.Active)
            .ToListAsync();
    }
}

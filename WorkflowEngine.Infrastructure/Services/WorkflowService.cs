using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WorkflowEngine.Core.DTOs;
using WorkflowEngine.Core.Entities;
using WorkflowEngine.Core.Enums;
using WorkflowEngine.Core.Interfaces;
using WorkflowEngine.Infrastructure.Data;

namespace WorkflowEngine.Infrastructure.Services;

public class WorkflowService : IWorkflowService
{
    private readonly AppDbContext _context;
    private readonly ILogger<WorkflowService> _logger;

    public WorkflowService(AppDbContext context, ILogger<WorkflowService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Guid> StartProcessAsync(string processCode, Guid userId)
    {
        _logger.LogInformation("Starting process {ProcessCode} for user {UserId}", processCode, userId);

        var process = await _context.Processes
            .FirstOrDefaultAsync(p => p.Code == processCode && p.IsActive);

        if (process == null)
        {
            _logger.LogWarning("Process not found or inactive: {ProcessCode}", processCode);
            throw new Exception($"Process not found or inactive: {processCode}");
        }

        var startStep = await _context.ProcessSteps
            .FirstOrDefaultAsync(s => s.ProcessId == process.Id && s.StepType == ProcessStepType.Start);

        if (startStep == null)
        {
            _logger.LogError("Start step not found for process: {ProcessCode}", processCode);
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

        _logger.LogInformation("Process started successfully. RequestId: {RequestId}, RequestNumber: {RequestNumber}", request.Id, requestNumber);
        return request.Id;
    }

    public async Task ExecuteActionAsync(ExecuteActionDto dto)
    {
        _logger.LogInformation("Executing action for Request {RequestId} by User {UserId}", dto.RequestId, dto.UserId);

        // 1. Get Request
        var request = await _context.ProcessRequests
            .Include(e => e.CurrentStep)
            .ThenInclude(s => s.Actions)
            .ThenInclude(a => a.Conditions) // Include conditions for Rule Engine
            .FirstOrDefaultAsync(e => e.Id == dto.RequestId);

        if (request == null)
        {
            _logger.LogWarning("Process Request not found: {RequestId}", dto.RequestId);
            throw new Exception($"Process Request not found: {dto.RequestId}");
        }

        if (request.Status != ProcessRequestStatus.Active)
        {
            _logger.LogWarning("Process Request is not active. Status: {Status}", request.Status);
            throw new Exception($"Process Request is not active. Status: {request.Status}");
        }

        // 2. Validate User Role (Mock: Check existence)
        var user = await _context.WebUsers.FindAsync(dto.UserId);
        if (user == null || !user.IsActive)
        {
            _logger.LogWarning("User not authorized or inactive: {UserId}", dto.UserId);
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
            _logger.LogWarning("Action not available in step '{StepName}'", request.CurrentStep.Name);
            throw new Exception($"Action not available in step '{request.CurrentStep.Name}'");
        }

        // 3.1 Validate Comment Requirement
        if (action.IsCommentRequired && string.IsNullOrWhiteSpace(dto.Comments))
        {
            _logger.LogWarning("Missing required comment for action: {ActionName}", action.Name);
            throw new Exception($"Comment is required for action: {action.Name}");
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
                    case ProcessEntryType.Select:
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

        foreach (var condition in action.Conditions)
        {
            if (condition.RuleExpression == "true") // Mock
            {
                if (condition.TargetStepId.HasValue)
                {
                    targetStepId = condition.TargetStepId.Value;
                    break;
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
            Comments = !string.IsNullOrWhiteSpace(dto.Comments) ? dto.Comments : $"Executed action: {action.Name}",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = dto.UserId.ToString()
        };

        _context.ProcessRequestHistories.Add(history);

        await _context.SaveChangesAsync();
        _logger.LogInformation("Action executed successfully for Request {RequestId}", dto.RequestId);
    }

    public async Task<List<ProcessRequest>> GetUserTasksAsync(Guid userId)
    {
        return await _context.ProcessRequests
            .Include(e => e.Process)
            .Include(e => e.CurrentStep)
            .Where(e => e.Status == ProcessRequestStatus.Active)
            .ToListAsync();
    }

    public async Task<List<ProcessHistoryDto>> GetRequestHistoryAsync(Guid requestId)
    {
        var history = await _context.ProcessRequestHistories
            .Include(h => h.ActorUser)
            .Include(h => h.Action)
            .Where(h => h.ProcessRequestId == requestId)
            .OrderByDescending(h => h.ActionTime)
            .Select(h => new ProcessHistoryDto
            {
                ActionName = h.Action != null ? h.Action.Name : "System",
                ActorName = h.ActorUser.Username,
                Description = h.Comments,
                CreatedAt = h.ActionTime
            })
            .ToListAsync();

        return history;
    }
}

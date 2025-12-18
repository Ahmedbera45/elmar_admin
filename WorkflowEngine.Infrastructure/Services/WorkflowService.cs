using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
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
    private readonly RuleEvaluator _ruleEvaluator;

    private static readonly ConcurrentDictionary<Guid, SemaphoreSlim> _locks = new();

    public WorkflowService(AppDbContext context, ILogger<WorkflowService> logger)
    {
        _context = context;
        _logger = logger;
        _ruleEvaluator = new RuleEvaluator();
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

        // Phase 7: RBAC Check
        var user = await _context.WebUsers.FindAsync(userId);
        if (user == null) throw new UnauthorizedAccessException("User not found.");

        if (!string.IsNullOrEmpty(process.AllowedRoles))
        {
            try
            {
                var allowedRoles = JsonSerializer.Deserialize<List<string>>(process.AllowedRoles);
                if (allowedRoles != null && allowedRoles.Any() && !allowedRoles.Contains(user.Role))
                {
                    _logger.LogWarning("User {UserId} with role {Role} not allowed to start process {ProcessCode}", userId, user.Role, processCode);
                    throw new UnauthorizedAccessException("You do not have permission to start this process.");
                }
            }
            catch (JsonException)
            {
                _logger.LogError("Invalid JSON in AllowedRoles for process {ProcessId}", process.Id);
            }
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
        var semaphore = _locks.GetOrAdd(dto.RequestId, _ => new SemaphoreSlim(1, 1));
        await semaphore.WaitAsync();

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            _logger.LogInformation("Executing action for Request {RequestId} by User {UserId}", dto.RequestId, dto.UserId);

            var request = await _context.ProcessRequests
                .Include(e => e.CurrentStep)
                .ThenInclude(s => s.Actions)
                .ThenInclude(a => a.Conditions)
                .FirstOrDefaultAsync(e => e.Id == dto.RequestId);

            if (request == null)
            {
                throw new Exception($"Process Request not found: {dto.RequestId}");
            }

            if (request.Status != ProcessRequestStatus.Active)
            {
                throw new Exception($"Process Request is not active. Status: {request.Status}");
            }

            var user = await _context.WebUsers.FindAsync(dto.UserId);
            if (user == null || !user.IsActive)
            {
                throw new UnauthorizedAccessException("User not authorized or inactive.");
            }

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

            // Phase 7: Withdraw Logic
            if (action.ActionType == ProcessActionType.Withdraw)
            {
                if (request.InitiatorUserId != dto.UserId && user.Role != "Admin")
                {
                    throw new UnauthorizedAccessException("Only the initiator or an Admin can withdraw the request.");
                }

                request.Status = ProcessRequestStatus.Cancelled;

                var cancelHistory = new ProcessRequestHistory
                {
                    ProcessRequestId = request.Id,
                    FromStepId = request.CurrentStepId,
                    ToStepId = request.CurrentStepId,
                    ActionId = action.Id,
                    ActorUserId = dto.UserId,
                    ActionTime = DateTime.UtcNow,
                    Comments = !string.IsNullOrWhiteSpace(dto.Comments) ? dto.Comments : "Process Withdrawn by User",
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = dto.UserId.ToString()
                };

                _context.ProcessRequestHistories.Add(cancelHistory);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return;
            }

            if (action.IsCommentRequired && string.IsNullOrWhiteSpace(dto.Comments))
            {
                throw new Exception($"Comment is required for action: {action.Name}");
            }

            var stepConnections = await _context.PePsConnections
                .Include(c => c.ProcessEntry)
                .Where(c => c.ProcessStepId == request.CurrentStepId)
                .ToListAsync();

            foreach (var connection in stepConnections)
            {
                if (connection.ProcessEntry.IsRequired && connection.PermissionType == ProcessEntryPermissionType.Write)
                {
                    if (!dto.FormValues.ContainsKey(connection.ProcessEntry.Key))
                    {
                        throw new Exception($"Missing required field: {connection.ProcessEntry.Title} ({connection.ProcessEntry.Key})");
                    }
                }

                if (dto.FormValues.TryGetValue(connection.ProcessEntry.Key, out var val) && val != null)
                {
                    if (!string.IsNullOrWhiteSpace(connection.ProcessEntry.ValidationRegex))
                    {
                        if (!Regex.IsMatch(val.ToString() ?? "", connection.ProcessEntry.ValidationRegex))
                        {
                            throw new System.ComponentModel.DataAnnotations.ValidationException(
                                $"Invalid format for field {connection.ProcessEntry.Title}. Value does not match pattern.");
                        }
                    }

                    var entryValue = new ProcessRequestValue
                    {
                        ProcessRequestId = request.Id,
                        ProcessEntryId = connection.ProcessEntry.Id,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = dto.UserId.ToString()
                    };

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

            Guid? targetStepId = action.TargetStepId;

            foreach (var condition in action.Conditions)
            {
                if (_ruleEvaluator.Evaluate(condition.RuleExpression, dto.FormValues))
                {
                    if (condition.TargetStepId.HasValue)
                    {
                        targetStepId = condition.TargetStepId.Value;
                        break;
                    }
                }
            }

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
            await transaction.CommitAsync();

            _logger.LogInformation("Action executed successfully for Request {RequestId}", dto.RequestId);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
        finally
        {
            semaphore.Release();
        }
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
        return await _context.ProcessRequestHistories
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
    }

    // Phase 7: Dynamic Views
    public async Task<ProcessViewDefinitionDto?> GetProcessViewDefinitionAsync(string processCode)
    {
        var process = await _context.Processes
            .Include(p => p.Steps)
            .FirstOrDefaultAsync(p => p.Code == processCode);

        if (process == null) return null;

        var listView = await _context.ProcessListViews
            .Include(lv => lv.Process)
            .FirstOrDefaultAsync(lv => lv.ProcessId == process.Id);

        if (listView == null) return null;

        var columns = await _context.ProcessListViewColumns
            .Where(c => c.ListViewId == listView.Id)
            .OrderBy(c => c.OrderIndex)
            .Select(c => new ProcessViewColumnDto
            {
                Key = c.ProcessEntryId,
                Title = c.Title,
                OrderIndex = c.OrderIndex,
                Width = c.Width
            })
            .ToListAsync();

        return new ProcessViewDefinitionDto
        {
            ProcessTitle = listView.Title,
            Columns = columns
        };
    }

    public async Task<List<ProcessRequestListDto>> GetProcessRequestsAsync(string processCode)
    {
        var process = await _context.Processes.FirstOrDefaultAsync(p => p.Code == processCode);
        if (process == null) return new List<ProcessRequestListDto>();

        var listView = await _context.ProcessListViews.FirstOrDefaultAsync(lv => lv.ProcessId == process.Id);
        var columns = listView != null
            ? await _context.ProcessListViewColumns.Where(c => c.ListViewId == listView.Id).ToListAsync()
            : new List<ProcessListViewColumn>();

        var requests = await _context.ProcessRequests
            .Where(r => r.ProcessId == process.Id)
            .ToListAsync();

        var result = new List<ProcessRequestListDto>();

        var requestIds = requests.Select(r => r.Id).ToList();
        var columnKeys = columns.Select(c => c.ProcessEntryId).ToList();

        var values = await _context.ProcessRequestValues
            .Include(v => v.ProcessEntry)
            .Where(v => requestIds.Contains(v.ProcessRequestId) && columnKeys.Contains(v.ProcessEntry.Key))
            .ToListAsync();

        foreach (var req in requests)
        {
            var dto = new ProcessRequestListDto
            {
                Id = req.Id,
                Status = req.Status,
                CreatedAt = req.CreatedAt,
                InitiatorUserId = req.InitiatorUserId
            };

            foreach (var col in columns)
            {
                var val = values
                    .OrderByDescending(v => v.CreatedAt)
                    .FirstOrDefault(v => v.ProcessRequestId == req.Id && v.ProcessEntry.Key == col.ProcessEntryId);

                if (val != null)
                {
                    object? objVal = null;
                    if (val.StringValue != null) objVal = val.StringValue;
                    else if (val.IntValue.HasValue) objVal = val.IntValue;
                    else if (val.DecimalValue.HasValue) objVal = val.DecimalValue;
                    else if (val.DateValue.HasValue) objVal = val.DateValue;
                    else if (val.BoolValue.HasValue) objVal = val.BoolValue;

                    dto.DynamicValues[col.ProcessEntryId] = objVal;
                }
                else
                {
                    dto.DynamicValues[col.ProcessEntryId] = null;
                }
            }
            result.Add(dto);
        }

        return result;
    }
}

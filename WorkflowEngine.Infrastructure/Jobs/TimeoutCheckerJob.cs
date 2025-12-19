using System;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WorkflowEngine.Core.DTOs;
using WorkflowEngine.Core.Enums;
using WorkflowEngine.Core.Interfaces;
using WorkflowEngine.Infrastructure.Data;

namespace WorkflowEngine.Infrastructure.Jobs;

public class TimeoutCheckerJob
{
    private readonly AppDbContext _context;
    private readonly IWorkflowService _workflowService;
    private readonly ILogger<TimeoutCheckerJob> _logger;

    public TimeoutCheckerJob(AppDbContext context, IWorkflowService workflowService, ILogger<TimeoutCheckerJob> logger)
    {
        _context = context;
        _workflowService = workflowService;
        _logger = logger;
    }

    public async Task CheckTimeoutsAsync()
    {
        _logger.LogInformation("Starting TimeoutCheckerJob...");

        // Find requests that are active
        var activeRequests = await _context.ProcessRequests
            .Include(r => r.CurrentStep)
            .ThenInclude(s => s.Actions)
            .Where(r => r.Status == ProcessRequestStatus.Active)
            .ToListAsync();

        // Ensure we have a system user for executing actions
        var systemUser = await _context.WebUsers.FirstOrDefaultAsync(u => u.Role == "Admin"); // Or a specific "System" user
        if (systemUser == null)
        {
             _logger.LogError("No system user found to execute timeout actions.");
             return;
        }

        foreach (var request in activeRequests)
        {
            // Phase 11: Updated Timeout Logic (ProcessStep based)

            bool isTimedOut = false;
            Guid? actionToExecute = null;

            // 1. Check Step Level Timeout
            if (request.CurrentStep.TimeoutHours.HasValue && request.CurrentStep.TimeoutHours.Value > 0)
            {
                var stepStartTime = request.ModifiedAt ?? request.CreatedAt;
                var timeoutTime = stepStartTime.AddHours(request.CurrentStep.TimeoutHours.Value);

                if (DateTime.UtcNow > timeoutTime)
                {
                    isTimedOut = true;
                    actionToExecute = request.CurrentStep.TimeoutActionId;
                    _logger.LogWarning("Request {RequestNumber} timed out on step {StepName} (Step Timeout)", request.RequestNumber, request.CurrentStep.Name);
                }
            }

            // 2. Check Action Level Timeout
            if (!isTimedOut)
            {
                var timeoutAction = request.CurrentStep.Actions
                    .FirstOrDefault(a => a.TimeoutSeconds.HasValue && a.TimeoutSeconds.Value > 0);

                if (timeoutAction != null)
                {
                    var timeoutSeconds = timeoutAction.TimeoutSeconds.GetValueOrDefault();
                    var lastActivity = request.ModifiedAt ?? request.CreatedAt;
                    var timeoutTime = lastActivity.AddSeconds(timeoutSeconds);

                    if (DateTime.UtcNow > timeoutTime)
                    {
                        isTimedOut = true;
                        actionToExecute = timeoutAction.TimeoutActionId;
                        _logger.LogWarning("Request {RequestNumber} timed out on step {StepName} (Action Timeout)", request.RequestNumber, request.CurrentStep.Name);
                    }
                }
            }

            if (isTimedOut && actionToExecute.HasValue)
            {
                 try
                 {
                     await _workflowService.ExecuteActionAsync(new ExecuteActionDto
                     {
                         RequestId = request.Id,
                         ActionId = actionToExecute.Value,
                         UserId = systemUser.Id,
                         Comments = "System: Auto-executed due to timeout."
                     });

                     _logger.LogInformation("Executed timeout action for Request {RequestNumber}", request.RequestNumber);
                 }
                 catch (Exception ex)
                 {
                     _logger.LogError(ex, "Failed to execute timeout action for Request {RequestNumber}", request.RequestNumber);
                 }
            }
        }

        // SLA Check (Overdue Requests)
        var overdueRequests = await _context.ProcessRequests
            .Where(r => r.Status == ProcessRequestStatus.Active && r.DueDate.HasValue && r.DueDate.Value < DateTime.UtcNow)
            .ToListAsync();

        foreach (var request in overdueRequests)
        {
             _logger.LogWarning("Request {RequestNumber} breached SLA. Auto-cancelling.", request.RequestNumber);
             request.Status = ProcessRequestStatus.Cancelled;

             var history = new ProcessRequestHistory
             {
                 ProcessRequestId = request.Id,
                 FromStepId = request.CurrentStepId,
                 ToStepId = request.CurrentStepId,
                 ActorUserId = systemUser.Id,
                 ActionTime = DateTime.UtcNow,
                 Comments = "System: Auto-cancelled due to SLA breach (Overdue)",
                 CreatedAt = DateTime.UtcNow,
                 CreatedBy = systemUser.Id.ToString()
             };
             _context.ProcessRequestHistories.Add(history);
        }

        if (overdueRequests.Any())
        {
            await _context.SaveChangesAsync();
        }

        _logger.LogInformation("TimeoutCheckerJob finished.");
    }
}

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
            var timeoutAction = request.CurrentStep.Actions
                .FirstOrDefault(a => a.TimeoutSeconds.HasValue && a.TimeoutSeconds.Value > 0);

            if (timeoutAction != null)
            {
                var timeoutSeconds = timeoutAction.TimeoutSeconds.GetValueOrDefault();
                var lastActivity = request.ModifiedAt ?? request.CreatedAt;
                var timeoutTime = lastActivity.AddSeconds(timeoutSeconds);

                if (DateTime.UtcNow > timeoutTime)
                {
                    _logger.LogWarning("Request {RequestNumber} timed out on step {StepName}", request.RequestNumber, request.CurrentStep.Name);

                    if (timeoutAction.TimeoutActionId.HasValue)
                    {
                         try
                         {
                             // Execute the Timeout Action
                             // Note: TimeoutActionId points to another Action definition that should be executed.
                             // Usually, this 'Timeout Action' is a special action (e.g., 'AutoReject') available on the step.
                             // Or it refers to the ID of the action to trigger.

                             // Logic: The configuration says "If this action times out, trigger THAT action".
                             // Let's assume TimeoutActionId IS the ID of the action to execute.

                             await _workflowService.ExecuteActionAsync(new ExecuteActionDto
                             {
                                 RequestId = request.Id,
                                 ActionId = timeoutAction.TimeoutActionId.Value,
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
            }
        }

        _logger.LogInformation("TimeoutCheckerJob finished.");
    }
}

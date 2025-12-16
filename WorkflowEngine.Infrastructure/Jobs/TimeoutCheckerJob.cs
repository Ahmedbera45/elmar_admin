using System;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WorkflowEngine.Core.Enums;
using WorkflowEngine.Infrastructure.Data;

namespace WorkflowEngine.Infrastructure.Jobs;

public class TimeoutCheckerJob
{
    private readonly AppDbContext _context;
    private readonly ILogger<TimeoutCheckerJob> _logger;

    public TimeoutCheckerJob(AppDbContext context, ILogger<TimeoutCheckerJob> logger)
    {
        _context = context;
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

        foreach (var request in activeRequests)
        {
            // Find action with timeout in current step
            // Assuming multiple actions, we pick the one with timeout configured.
            // If multiple have timeout, business logic needed. For now, check any.
            var timeoutAction = request.CurrentStep.Actions
                .FirstOrDefault(a => a.TimeoutSeconds.HasValue && a.TimeoutSeconds.Value > 0);

            if (timeoutAction != null)
            {
                var timeoutSeconds = timeoutAction.TimeoutSeconds.GetValueOrDefault();
                var timeoutTime = (request.ModifiedAt ?? request.CreatedAt).AddSeconds(timeoutSeconds);

                if (DateTime.UtcNow > timeoutTime)
                {
                    _logger.LogWarning("Request {RequestNumber} timed out on step {StepName}", request.RequestNumber, request.CurrentStep.Name);

                    // Logic to execute Timeout Action or Auto-reject?
                    // Prompt says: "Zaman Aşımı logu atacak (şimdilik) bir metod yaz"
                    // So just logging is enough for this task.
                    // Future: Trigger TimeoutActionId
                }
            }
        }

        _logger.LogInformation("TimeoutCheckerJob finished.");
    }
}

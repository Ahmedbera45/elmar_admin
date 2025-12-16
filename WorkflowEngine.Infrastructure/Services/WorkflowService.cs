using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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
        // 1. Find the active process definition
        var process = await _context.Processes
            .Include(p => p.Steps)
            .FirstOrDefaultAsync(p => p.Code == processCode && p.IsActive);

        if (process == null)
        {
            throw new Exception($"Process not found or inactive: {processCode}");
        }

        // 2. Find the Start Step
        var startStep = await _context.ProcessSteps
            .FirstOrDefaultAsync(s => s.ProcessId == process.Id && s.StepType == ProcessStepType.Start);

        if (startStep == null)
        {
            throw new Exception($"Start step not found for process: {processCode}");
        }

        // 3. Generate Request Number (Renamed from EntryNumber)
        var requestNumber = $"PR-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";

        // 4. Create Process Request (Renamed from ProcessEntry)
        var request = new ProcessRequest
        {
            ProcessId = process.Id,
            CurrentStepId = startStep.Id,
            Status = ProcessEntryStatus.Active, // Still utilizing the same enum for status
            InitiatorUserId = userId,
            RequestNumber = requestNumber,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId.ToString()
        };

        _context.ProcessRequests.Add(request);

        // 5. Create History Log
        var history = new ProcessRequestHistory
        {
            ProcessRequestId = request.Id,
            ToStepId = startStep.Id,
            ActorUserId = userId,
            ActionTime = DateTime.UtcNow,
            Comments = "Süreç Başlatıldı",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId.ToString()
        };

        history.ProcessRequest = request;
        _context.ProcessRequestHistories.Add(history);

        await _context.SaveChangesAsync();

        return request.Id;
    }

    public async Task ExecuteActionAsync(Guid requestId, string actionName, Guid userId, Dictionary<string, object> inputs)
    {
        // 1. Get Request with current step and its actions
        var request = await _context.ProcessRequests
            .Include(e => e.CurrentStep)
            .ThenInclude(s => s.Actions)
            .FirstOrDefaultAsync(e => e.Id == requestId);

        if (request == null)
        {
            throw new Exception($"Process Request not found: {requestId}");
        }

        if (request.Status != ProcessEntryStatus.Active)
        {
            throw new Exception($"Process Request is not active. Status: {request.Status}");
        }

        // 2. Find the action in the current step
        var action = request.CurrentStep.Actions
            .FirstOrDefault(a => a.Name.Equals(actionName, StringComparison.OrdinalIgnoreCase));

        if (action == null)
        {
            throw new Exception($"Action '{actionName}' not available in step '{request.CurrentStep.Name}'");
        }

        // 3. Determine target step
        Guid? targetStepId = action.TargetStepId;

        // 4. Update Request
        var previousStepId = request.CurrentStepId;

        if (targetStepId.HasValue)
        {
            request.CurrentStepId = targetStepId.Value;

            // Check if target step is an End step
            var targetStep = await _context.ProcessSteps.FindAsync(targetStepId.Value);
            if (targetStep != null && targetStep.StepType == ProcessStepType.End)
            {
                request.Status = ProcessEntryStatus.Completed;
            }
        }

        // 5. Create History Log
        var history = new ProcessRequestHistory
        {
            ProcessRequestId = request.Id,
            FromStepId = previousStepId,
            ToStepId = targetStepId ?? previousStepId,
            ActionId = action.Id,
            ActorUserId = userId,
            ActionTime = DateTime.UtcNow,
            Comments = $"Executed action: {actionName}",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId.ToString()
        };

        _context.ProcessRequestHistories.Add(history);

        await _context.SaveChangesAsync();
    }

    public async Task<List<ProcessRequest>> GetUserTasksAsync(Guid userId)
    {
        // Return active requests
        return await _context.ProcessRequests
            .Include(e => e.Process)
            .Include(e => e.CurrentStep)
            .Where(e => e.Status == ProcessEntryStatus.Active) // Simple filtering
            .ToListAsync();
    }
}

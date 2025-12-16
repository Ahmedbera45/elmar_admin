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

        // 3. Generate Entry Number (Simple implementation)
        var entryNumber = $"PR-{DateTime.UtcNow.Year}-{new Random().Next(1000, 9999)}";

        // 4. Create Process Entry
        var entry = new ProcessEntry
        {
            ProcessId = process.Id,
            CurrentStepId = startStep.Id,
            Status = ProcessEntryStatus.Active,
            InitiatorUserId = userId,
            EntryNumber = entryNumber,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId.ToString()
        };

        _context.ProcessEntries.Add(entry);

        // 5. Create History Log
        var history = new ProcessEntryHistory
        {
            ProcessEntryId = entry.Id, // Will be set after SaveChanges, but EF Core handles this if added to context
            // No FromStepId for start
            ToStepId = startStep.Id,
            ActorUserId = userId,
            ActionTime = DateTime.UtcNow,
            Comments = "Process Started",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId.ToString()
        };

        // Linking navigation property to ensure ID propagation if needed before save,
        // though EF usually handles it. Explicitly adding to context is safer.
        history.ProcessEntry = entry;
        _context.ProcessEntryHistories.Add(history);

        await _context.SaveChangesAsync();

        return entry.Id;
    }

    public async Task ExecuteActionAsync(Guid entryId, string actionName, Guid userId, Dictionary<string, object> inputs)
    {
        // 1. Get Entry with current step and its actions
        var entry = await _context.ProcessEntries
            .Include(e => e.CurrentStep)
            .ThenInclude(s => s.Actions)
            .FirstOrDefaultAsync(e => e.Id == entryId);

        if (entry == null)
        {
            throw new Exception($"Process Entry not found: {entryId}");
        }

        if (entry.Status != ProcessEntryStatus.Active)
        {
            throw new Exception($"Process Entry is not active. Status: {entry.Status}");
        }

        // 2. Find the action in the current step
        var action = entry.CurrentStep.Actions
            .FirstOrDefault(a => a.Name.Equals(actionName, StringComparison.OrdinalIgnoreCase));

        if (action == null)
        {
            throw new Exception($"Action '{actionName}' not available in step '{entry.CurrentStep.Name}'");
        }

        // 3. Determine target step
        Guid? targetStepId = action.TargetStepId;

        // 4. Update Entry
        var previousStepId = entry.CurrentStepId;

        if (targetStepId.HasValue)
        {
            entry.CurrentStepId = targetStepId.Value;

            // Optional: Check if target step is an End step to mark process as completed?
            // For now, we assume standard transition.
            // We should fetch target step type to be precise, but for "Happy Path" instructions we update ID.

            // Let's quickly check if the target step is an End step to auto-complete
            var targetStep = await _context.ProcessSteps.FindAsync(targetStepId.Value);
            if (targetStep != null && targetStep.StepType == ProcessStepType.End)
            {
                entry.Status = ProcessEntryStatus.Completed;
            }
        }

        // 5. Create History Log
        var history = new ProcessEntryHistory
        {
            ProcessEntryId = entry.Id,
            FromStepId = previousStepId,
            ToStepId = targetStepId ?? previousStepId,
            ActionId = action.Id,
            ActorUserId = userId,
            ActionTime = DateTime.UtcNow,
            Comments = $"Executed action: {actionName}",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId.ToString()
        };

        _context.ProcessEntryHistories.Add(history);

        await _context.SaveChangesAsync();
    }

    public async Task<List<ProcessEntry>> GetUserTasksAsync(Guid userId)
    {
        // Simple implementation: Return active entries where user is initiator
        // In a real system, this would check "Assignments" or "Roles" for the current step.
        return await _context.ProcessEntries
            .Include(e => e.Process)
            .Include(e => e.CurrentStep)
            .Where(e => e.InitiatorUserId == userId && e.Status == ProcessEntryStatus.Active)
            .ToListAsync();
    }
}

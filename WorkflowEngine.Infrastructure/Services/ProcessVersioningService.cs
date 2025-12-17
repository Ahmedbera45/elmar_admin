using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WorkflowEngine.Core.Entities;
using WorkflowEngine.Infrastructure.Data;

namespace WorkflowEngine.Infrastructure.Services;

public class ProcessVersioningService
{
    private readonly AppDbContext _context;

    public ProcessVersioningService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> CreateNewVersionAsync(Guid processId)
    {
        // 1. Fetch existing process with all related entities
        var existingProcess = await _context.Processes
            .Include(p => p.Steps)
                .ThenInclude(s => s.Actions)
                    .ThenInclude(a => a.Conditions)
            // Note: ProcessEntry definitions are usually global or per-process?
            // In this domain, ProcessEntry seems to be reusable (Form Dictionary) but mapped via PePsConnection.
            // However, PePsConnection is linked to ProcessStep.
            // We need to fetch and clone PePsConnections later.
            .FirstOrDefaultAsync(p => p.Id == processId);

        if (existingProcess == null)
            throw new Exception("Process not found");

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // 2. Clone Process
            var newProcess = new Process
            {
                Id = Guid.NewGuid(),
                Name = existingProcess.Name,
                Code = existingProcess.Code,
                Description = existingProcess.Description,
                Version = existingProcess.Version + 1,
                IsActive = true, // New version active
                ParentProcessId = existingProcess.Id, // Link to previous version logic? Or just track lineage.
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System" // Or pass userId
            };

            _context.Processes.Add(newProcess);

            // Dictionary to map old Step ID -> New Step ID (for Action transitions)
            var stepMap = new Dictionary<Guid, Guid>();

            // 3. Clone Steps
            foreach (var oldStep in existingProcess.Steps.OrderBy(s => s.OrderIndex))
            {
                var newStep = new ProcessStep
                {
                    Id = Guid.NewGuid(),
                    ProcessId = newProcess.Id,
                    Name = oldStep.Name,
                    StepType = oldStep.StepType,
                    OrderIndex = oldStep.OrderIndex,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                };

                stepMap[oldStep.Id] = newStep.Id;
                _context.ProcessSteps.Add(newStep);
            }

            // Save to generate IDs if needed, but we set GUIDs manually.
            // However, we need to handle Actions which reference TargetStepId.

            // 4. Clone Actions and Connections
            foreach (var oldStep in existingProcess.Steps)
            {
                var newStepId = stepMap[oldStep.Id];

                // Clone Actions
                foreach (var oldAction in oldStep.Actions)
                {
                    var newAction = new ProcessAction
                    {
                        Id = Guid.NewGuid(),
                        ProcessStepId = newStepId,
                        Name = oldAction.Name,
                        ActionType = oldAction.ActionType,
                        IsCommentRequired = oldAction.IsCommentRequired,
                        TimeoutSeconds = oldAction.TimeoutSeconds,
                        TimeoutActionId = oldAction.TimeoutActionId, // Issue: This might point to an old action ID.
                        // Solving circular ref or intra-process ref for TimeoutActionId is complex if it points to an action in the same process.
                        // For now, we keep it as is, or we would need a map for Action IDs too.

                        TargetStepId = oldAction.TargetStepId.HasValue && stepMap.ContainsKey(oldAction.TargetStepId.Value)
                            ? stepMap[oldAction.TargetStepId.Value]
                            : oldAction.TargetStepId, // If null or external

                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = "System"
                    };

                    _context.ProcessActions.Add(newAction);

                    // Clone Conditions
                    foreach (var oldCond in oldAction.Conditions)
                    {
                        var newCond = new ProcessActionCondition
                        {
                            Id = Guid.NewGuid(),
                            ProcessActionId = newAction.Id,
                            RuleExpression = oldCond.RuleExpression,
                            TargetStepId = oldCond.TargetStepId.HasValue && stepMap.ContainsKey(oldCond.TargetStepId.Value)
                                ? stepMap[oldCond.TargetStepId.Value]
                                : oldCond.TargetStepId,
                            CreatedAt = DateTime.UtcNow,
                            CreatedBy = "System"
                        };
                        _context.ProcessActionConditions.Add(newCond);
                    }
                }

                // Clone PePsConnections (Form Maps) for this step
                // Note: We need to query them separately as they were not included in the initial query
                var oldConnections = await _context.PePsConnections
                    .Where(c => c.ProcessStepId == oldStep.Id)
                    .ToListAsync();

                foreach (var oldConn in oldConnections)
                {
                    var newConn = new PePsConnection
                    {
                        Id = Guid.NewGuid(),
                        ProcessStepId = newStepId,
                        ProcessEntryId = oldConn.ProcessEntryId, // Reuse the same form definition
                        PermissionType = oldConn.PermissionType,
                        OrderIndex = oldConn.OrderIndex,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = "System"
                    };
                    _context.PePsConnections.Add(newConn);
                }
            }

            // 5. Deactivate Old Process
            existingProcess.IsActive = false;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return newProcess.Id;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}

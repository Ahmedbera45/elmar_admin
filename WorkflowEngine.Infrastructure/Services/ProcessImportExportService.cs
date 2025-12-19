using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WorkflowEngine.Core.Entities;
using WorkflowEngine.Infrastructure.Data;

namespace WorkflowEngine.Infrastructure.Services;

public class ProcessImportExportService
{
    private readonly AppDbContext _context;

    public ProcessImportExportService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<string> ExportProcessAsync(Guid processId)
    {
        var process = await _context.Processes
            .Include(p => p.Steps)
                .ThenInclude(s => s.Actions)
                    .ThenInclude(a => a.Conditions)
            .FirstOrDefaultAsync(p => p.Id == processId);

        if (process == null) throw new Exception("Process not found");

        // We also need to export the Form Definitions (ProcessEntries) used by this process
        // Find all ProcessEntries linked via PePsConnection
        var stepIds = process.Steps.Select(s => s.Id).ToList();
        var connections = await _context.PePsConnections
            .Include(c => c.ProcessEntry)
            .Where(c => stepIds.Contains(c.ProcessStepId))
            .ToListAsync();

        var entries = connections.Select(c => c.ProcessEntry).DistinctBy(e => e.Id).ToList();

        // Create a DTO for export
        var exportData = new ProcessExportDto
        {
            Process = process,
            Connections = connections.Select(c => new PePsConnectionExportDto
            {
                StepName = process.Steps.First(s => s.Id == c.ProcessStepId).Name, // Map by Name to be ID agnostic on import
                EntryKey = c.ProcessEntry.Key,
                PermissionType = c.PermissionType,
                OrderIndex = c.OrderIndex
            }).ToList(),
            Entries = entries
        };

        return JsonSerializer.Serialize(exportData, new JsonSerializerOptions { WriteIndented = true, ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve });
    }

    public async Task ImportProcessAsync(string json)
    {
        var data = JsonSerializer.Deserialize<ProcessExportDto>(json, new JsonSerializerOptions { ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve });
        if (data == null || data.Process == null) throw new Exception("Invalid import data");

        // Check if process exists by Code
        var existingProcess = await _context.Processes.FirstOrDefaultAsync(p => p.Code == data.Process.Code);

        // We will create a new version of the process, or a new process if it doesn't exist.
        // If it exists, we take its ID (or Code) and increment version.
        // Actually, we should probably treat it as a new import and maybe increment version if Code matches.

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // 1. Upsert ProcessEntries (Global)
            // If Entry with Key exists, check if it matches? Or update it?
            // Let's assume keys are unique global identifiers.
            foreach (var entry in data.Entries)
            {
                var existingEntry = await _context.ProcessEntries.FirstOrDefaultAsync(e => e.Key == entry.Key);
                if (existingEntry == null)
                {
                    entry.Id = Guid.NewGuid(); // New ID for this environment
                    _context.ProcessEntries.Add(entry);
                }
                else
                {
                    // Update definition? Or keep existing?
                    // Let's update definition to match import
                    existingEntry.Title = entry.Title;
                    existingEntry.EntryType = entry.EntryType;
                    existingEntry.Options = entry.Options;
                    existingEntry.ValidationRegex = entry.ValidationRegex;
                    existingEntry.MinValue = entry.MinValue;
                    existingEntry.MaxValue = entry.MaxValue;
                    existingEntry.ErrorMessage = entry.ErrorMessage;
                    // existingEntry.Id remains same
                }
            }
            await _context.SaveChangesAsync(); // Save entries to get IDs if needed or ensure they exist

            // 2. Create Process
            var newProcess = data.Process;
            newProcess.Id = Guid.NewGuid();
            newProcess.CreatedAt = DateTime.UtcNow;

            if (existingProcess != null)
            {
                // Versioning
                var maxVersion = await _context.Processes
                    .Where(p => p.Code == data.Process.Code)
                    .MaxAsync(p => (int?)p.Version) ?? 0;

                newProcess.Version = maxVersion + 1;
                // Deactivate old active ones? Usually import implies we want this to be the new active one.
                // But let's leave existing ones alone until verified.
                // Or user might want to set this as active.
                newProcess.IsActive = false; // Import as inactive draft?
            }
            else
            {
                newProcess.Version = 1;
                newProcess.IsActive = true;
            }

            // Fix IDs for Steps and Actions
            foreach (var step in newProcess.Steps)
            {
                step.Id = Guid.NewGuid();
                step.ProcessId = newProcess.Id;

                foreach (var action in step.Actions)
                {
                    action.Id = Guid.NewGuid();
                    action.ProcessStepId = step.Id;

                    foreach (var cond in action.Conditions)
                    {
                        cond.Id = Guid.NewGuid();
                        cond.ProcessActionId = action.Id;
                    }
                }
            }

            // Re-link TargetStepIds (They currently point to old IDs from export)
            // We need a map OldID -> NewID.
            // But data.Process.Steps has new IDs now. We need to know which one corresponds to which old one.
            // Since we modified the objects in place, we lost the old IDs if we overwrote them.
            // Actually, deserialization created NEW objects. The IDs in them are from the JSON (Old IDs).
            // So we should build the map BEFORE overwriting IDs.

            // Re-do step loop correctly
            var stepMap = new Dictionary<Guid, Guid>(); // Old -> New
            foreach (var step in newProcess.Steps)
            {
                var oldId = step.Id;
                step.Id = Guid.NewGuid();
                stepMap[oldId] = step.Id;
                step.ProcessId = newProcess.Id;
            }

            // Now fix Actions
             foreach (var step in newProcess.Steps)
            {
                foreach (var action in step.Actions)
                {
                    var oldActionId = action.Id;
                    action.Id = Guid.NewGuid();
                    action.ProcessStepId = step.Id;

                    if (action.TargetStepId.HasValue && stepMap.ContainsKey(action.TargetStepId.Value))
                    {
                        action.TargetStepId = stepMap[action.TargetStepId.Value];
                    }
                    else
                    {
                        action.TargetStepId = null; // Clear invalid link
                    }

                    // Conditions
                     foreach (var cond in action.Conditions)
                    {
                        cond.Id = Guid.NewGuid();
                        cond.ProcessActionId = action.Id;
                        if (cond.TargetStepId.HasValue && stepMap.ContainsKey(cond.TargetStepId.Value))
                        {
                            cond.TargetStepId = stepMap[cond.TargetStepId.Value];
                        }
                    }
                }
            }

            _context.Processes.Add(newProcess);
            await _context.SaveChangesAsync();

            // 3. Create Connections
            foreach (var connDto in data.Connections)
            {
                var step = newProcess.Steps.FirstOrDefault(s => s.Name == connDto.StepName); // Match by name
                var entry = await _context.ProcessEntries.FirstOrDefaultAsync(e => e.Key == connDto.EntryKey);

                if (step != null && entry != null)
                {
                    var conn = new PePsConnection
                    {
                        Id = Guid.NewGuid(),
                        ProcessStepId = step.Id,
                        ProcessEntryId = entry.Id,
                        PermissionType = connDto.PermissionType,
                        OrderIndex = connDto.OrderIndex
                    };
                    _context.PePsConnections.Add(conn);
                }
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}

public class ProcessExportDto
{
    public Process Process { get; set; }
    public List<PePsConnectionExportDto> Connections { get; set; }
    public List<ProcessEntry> Entries { get; set; }
}

public class PePsConnectionExportDto
{
    public string StepName { get; set; }
    public string EntryKey { get; set; }
    public WorkflowEngine.Core.Enums.ProcessEntryPermissionType PermissionType { get; set; }
    public int OrderIndex { get; set; }
}

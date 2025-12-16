using System;
using WorkflowEngine.Core.Common;
using WorkflowEngine.Core.Enums;

namespace WorkflowEngine.Core.Entities;

public class ProcessRequest : BaseEntity
{
    public Guid ProcessId { get; set; }
    public Guid CurrentStepId { get; set; }
    public ProcessEntryStatus Status { get; set; } // Rename Enum? User didn't ask, but "ProcessEntryStatus" -> "ProcessRequestStatus" might be better. User said "ProcessEntryStatus -> Active...". I will keep ProcessEntryStatus but rename usage if needed, or rename enum. The prompt says "Yeni Enum: ProcessEntryStatus", but that was Phase 3. Phase 4 renames ProcessEntry table. I'll stick to renaming the class usage.
    public Guid InitiatorUserId { get; set; }
    public required string RequestNumber { get; set; } // Renamed from EntryNumber to RequestNumber to match table name logic

    // Navigation Properties
    public Process Process { get; set; } = null!;
    public ProcessStep CurrentStep { get; set; } = null!;
    public WebUser InitiatorUser { get; set; } = null!;
}

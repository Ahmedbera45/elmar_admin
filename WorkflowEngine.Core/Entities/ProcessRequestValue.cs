using System;
using WorkflowEngine.Core.Common;

namespace WorkflowEngine.Core.Entities;

public class ProcessRequestValue : BaseEntity
{
    public Guid ProcessRequestId { get; set; }
    public Guid ProcessEntryId { get; set; }

    // Sparse Columns
    public string? StringValue { get; set; }
    public int? IntValue { get; set; }
    public decimal? DecimalValue { get; set; }
    public DateTime? DateValue { get; set; }
    public bool? BoolValue { get; set; }

    // Navigation Properties
    public ProcessRequest ProcessRequest { get; set; } = null!;
    public ProcessEntry ProcessEntry { get; set; } = null!;
}

using System;
using WorkflowEngine.Core.Common;
using WorkflowEngine.Core.Enums;

namespace WorkflowEngine.Core.Entities;

public class ProcessEntry : BaseEntity
{
    public required string Key { get; set; }
    public required string Title { get; set; }
    public ProcessEntryType EntryType { get; set; }
    public string? Options { get; set; } // JSON: [{"label":"A","value":"1"}]
    public bool IsRequired { get; set; }
    public string? ValidationRegex { get; set; }
    public decimal? MinValue { get; set; }
    public decimal? MaxValue { get; set; }
    public string? ErrorMessage { get; set; }
    public string? CalculationFormula { get; set; }
    public string? LookupSource { get; set; }

    public Guid? ExternalDatasetId { get; set; }
    public ExternalDataset? ExternalDataset { get; set; }
}

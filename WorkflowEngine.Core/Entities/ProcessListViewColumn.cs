using System;

namespace WorkflowEngine.Core.Entities;

public class ProcessListViewColumn : BaseEntity
{
    public Guid ListViewId { get; set; }
    public string ProcessEntryId { get; set; } = string.Empty; // Key in ProcessEntry
    public string Title { get; set; } = string.Empty;
    public int OrderIndex { get; set; }
    public string? Width { get; set; }

    public virtual ProcessListView ListView { get; set; } = null!;
}

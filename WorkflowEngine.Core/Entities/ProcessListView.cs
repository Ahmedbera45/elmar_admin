using System;
using WorkflowEngine.Core.Common;
namespace WorkflowEngine.Core.Entities;

public class ProcessListView : BaseEntity
{
    public Guid ProcessId { get; set; }
    public string Title { get; set; } = string.Empty;

    public virtual Process Process { get; set; } = null!;
}

using WorkflowEngine.Core.Common;

namespace WorkflowEngine.Core.Entities;

public class AppRole : BaseEntity
{
    public required string Name { get; set; }
    public string? Description { get; set; }
}

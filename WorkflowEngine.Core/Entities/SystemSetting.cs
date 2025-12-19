using WorkflowEngine.Core.Common;

namespace WorkflowEngine.Core.Entities;

public class SystemSetting : BaseEntity
{
    public required string Key { get; set; }
    public string? Value { get; set; }
    public string? Description { get; set; }
}

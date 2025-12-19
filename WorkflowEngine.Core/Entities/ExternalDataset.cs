using System;
using WorkflowEngine.Core.Common;

namespace WorkflowEngine.Core.Entities;

public class ExternalDataset : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public Guid ExternalConnectionId { get; set; }
    public string QueryTemplate { get; set; } = string.Empty;

    public virtual ExternalConnection? ExternalConnection { get; set; }
}

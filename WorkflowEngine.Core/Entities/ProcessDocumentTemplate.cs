using System;

namespace WorkflowEngine.Core.Entities;

public class ProcessDocumentTemplate : BaseEntity
{
    public Guid ProcessId { get; set; }
    public Guid? StepId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string HtmlTemplateContent { get; set; } = string.Empty;

    public virtual Process Process { get; set; } = null!;
    public virtual ProcessStep? Step { get; set; }
}

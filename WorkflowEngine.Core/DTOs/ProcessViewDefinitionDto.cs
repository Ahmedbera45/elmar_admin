using System.Collections.Generic;

namespace WorkflowEngine.Core.DTOs;

public class ProcessViewDefinitionDto
{
    public string ProcessTitle { get; set; } = string.Empty;
    public List<ProcessViewColumnDto> Columns { get; set; } = new List<ProcessViewColumnDto>();
}

public class ProcessViewColumnDto
{
    public string Key { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int OrderIndex { get; set; }
    public string? Width { get; set; }
}

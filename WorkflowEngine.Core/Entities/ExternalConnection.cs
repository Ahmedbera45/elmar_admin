using System;
using WorkflowEngine.Core.Common;

namespace WorkflowEngine.Core.Entities;

public class ExternalConnection : BaseEntity
{
    public required string Name { get; set; }
    public required string Provider { get; set; } // "MSSQL", "PostgreSQL"
    public required string ConnectionString { get; set; }
}

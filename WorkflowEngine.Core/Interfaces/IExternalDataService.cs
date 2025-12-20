using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WorkflowEngine.Core.Interfaces;

public interface IExternalDataService
{
    Task<bool> TestConnectionAsync(Guid connectionId);
    Task<List<Dictionary<string, object>>> ExecuteDatasetAsync(Guid datasetId, Dictionary<string, object> parameters);
}

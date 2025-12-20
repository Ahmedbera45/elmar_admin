using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WorkflowEngine.Core.Interfaces;
using WorkflowEngine.Infrastructure.Data;

namespace WorkflowEngine.Infrastructure.Services;

public class ExternalDataService : IExternalDataService
{
    private readonly AppDbContext _context;

    public ExternalDataService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> TestConnectionAsync(Guid connectionId)
    {
        var conn = await _context.ExternalConnections.FindAsync(connectionId);
        if (conn == null) throw new Exception("Connection not found");
        // Mock connection test
        return true;
    }

    public async Task<List<Dictionary<string, object>>> ExecuteDatasetAsync(Guid datasetId, Dictionary<string, object> parameters)
    {
         var ds = await _context.ExternalDatasets.FindAsync(datasetId);
         if (ds == null) throw new Exception("Dataset not found");

         // Mock execution
         return new List<Dictionary<string, object>> {
             new Dictionary<string, object> { { "Id", 1 }, { "Name", "Test Item" } }
         };
    }
}

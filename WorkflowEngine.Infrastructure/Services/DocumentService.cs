using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WorkflowEngine.Core.Common;
using WorkflowEngine.Core.Interfaces;
using WorkflowEngine.Infrastructure.Data;

namespace WorkflowEngine.Infrastructure.Services;

public class DocumentService : IDocumentService
{
    private readonly AppDbContext _context;

    public DocumentService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<string> GenerateDocumentAsync(Guid templateId, Guid requestId)
    {
        var template = await _context.ProcessDocumentTemplates.FindAsync(templateId);
        if (template == null) throw new Exception("Template not found");

        var values = await _context.ProcessRequestValues
            .Include(v => v.ProcessEntry)
            .Where(v => v.ProcessRequestId == requestId)
            .ToListAsync();

        var request = await _context.ProcessRequests
            .Include(r => r.InitiatorUser)
            .Include(r => r.Process)
            .FirstOrDefaultAsync(r => r.Id == requestId);

        if (request == null) throw new Exception("Request not found");

        var dictionary = new Dictionary<string, object?>();

        // Add System Variables
        dictionary["RequestNumber"] = request.RequestNumber;
        dictionary["ProcessName"] = request.Process.Name;
        dictionary["Initiator"] = request.InitiatorUser.Username;
        dictionary["Date"] = DateTime.UtcNow.ToString("yyyy-MM-dd");

        // Add Form Values
        foreach (var v in values)
        {
            object? val = v.StringValue;
            if (v.IntValue.HasValue) val = v.IntValue;
            else if (v.DecimalValue.HasValue) val = v.DecimalValue;
            else if (v.DateValue.HasValue) val = v.DateValue;
            else if (v.BoolValue.HasValue) val = v.BoolValue;

            dictionary[v.ProcessEntry.Key] = val;
        }

        return TemplateHelper.ReplacePlaceholders(template.HtmlTemplateContent, dictionary);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkflowEngine.Core.Entities;
using WorkflowEngine.Infrastructure.Data;

namespace WorkflowEngine.API.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/reports")]
public class ReportController : ControllerBase
{
    private readonly AppDbContext _context;

    public ReportController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost("generate")]
    public async Task<IActionResult> GenerateReport([FromBody] ReportRequestDto dto)
    {
        // 1. Base Query: ProcessRequests
        var query = _context.ProcessRequests.AsQueryable();

        // 2. Filter (if any)
        if (dto.StartDate.HasValue) query = query.Where(r => r.CreatedAt >= dto.StartDate);
        if (dto.EndDate.HasValue) query = query.Where(r => r.CreatedAt <= dto.EndDate);
        if (!string.IsNullOrEmpty(dto.ProcessCode))
            query = query.Where(r => r.Process.Code == dto.ProcessCode);

        // 3. Group By X-Axis
        if (dto.XAxis == "Status")
        {
            var data = await query
                .GroupBy(r => r.Status)
                .Select(g => new { Label = g.Key.ToString(), Value = g.Count() }) // Y-Axis: Count (Default)
                .ToListAsync();
            return Ok(data);
        }
        else if (dto.XAxis == "Date")
        {
             var data = await query
                .GroupBy(r => r.CreatedAt.Date)
                .Select(g => new { Label = g.Key, Value = g.Count() })
                .OrderBy(d => d.Label)
                .ToListAsync();
             return Ok(data);
        }
        else
        {
            // Custom Field (ProcessEntry Key)
            // We need to join ProcessRequestValues
            var data = await _context.ProcessRequestValues
                .Where(v => v.ProcessEntry.Key == dto.XAxis)
                .Where(v => query.Select(q => q.Id).Contains(v.ProcessRequestId)) // Filter by request query
                .GroupBy(v => v.StringValue ?? (v.IntValue.HasValue ? v.IntValue.ToString() : "N/A"))
                .Select(g => new { Label = g.Key, Value = g.Count() })
                .ToListAsync();

            return Ok(data);
        }
    }
}

public class ReportRequestDto
{
    public string XAxis { get; set; } // "Status", "Date", or "FieldKey"
    public string YAxis { get; set; } // "Count" (Default), "Sum" (Not impl yet)
    public string? ProcessCode { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string ChartType { get; set; } // "Bar", "Pie"
}

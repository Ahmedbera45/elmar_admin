using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WorkflowEngine.Core.DTOs;
using WorkflowEngine.Core.Enums;
using WorkflowEngine.Core.Interfaces;
using WorkflowEngine.Infrastructure.Data;

namespace WorkflowEngine.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private readonly AppDbContext _context;

    public DashboardService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardStatsDto> GetStatsAsync(Guid userId, string role)
    {
        var query = _context.ProcessRequests.AsNoTracking();

        if (role != "Admin")
        {
            query = query.Where(r => r.InitiatorUserId == userId);
        }

        var stats = await query
            .GroupBy(r => r.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        var dto = new DashboardStatsDto();
        foreach (var stat in stats)
        {
            if (stat.Status == ProcessRequestStatus.Active) dto.Pending += stat.Count;
            else if (stat.Status == ProcessRequestStatus.Completed) dto.Approved += stat.Count;
            else if (stat.Status == ProcessRequestStatus.Rejected) dto.Rejected += stat.Count;
        }

        dto.Total = await query.CountAsync();

        return dto;
    }

    public async Task<List<ChartDataDto>> GetChartDataAsync(Guid userId, string role)
    {
        var query = _context.ProcessRequests.AsNoTracking();

        if (role != "Admin")
        {
            query = query.Where(r => r.InitiatorUserId == userId);
        }

        var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);
        query = query.Where(r => r.CreatedAt >= sixMonthsAgo);

        var rawData = await query
            .Select(r => r.CreatedAt)
            .ToListAsync();

        var result = new List<ChartDataDto>();
        for (int i = 5; i >= 0; i--)
        {
            var date = DateTime.UtcNow.AddMonths(-i);
            var monthName = date.ToString("MMM");
            var count = rawData.Count(d => d.Year == date.Year && d.Month == date.Month);
            result.Add(new ChartDataDto { Name = monthName, Count = count });
        }

        return result;
    }
}

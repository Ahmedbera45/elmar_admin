using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using WorkflowEngine.Core.DTOs;
using WorkflowEngine.Core.Entities;
using WorkflowEngine.Core.Enums;
using WorkflowEngine.Core.Interfaces;
using WorkflowEngine.Infrastructure.Data;

namespace WorkflowEngine.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestScenarioController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IWorkflowService _workflowService;
    private readonly IWebHostEnvironment _env;

    public TestScenarioController(AppDbContext context, IWorkflowService workflowService, IWebHostEnvironment env)
    {
        _context = context;
        _workflowService = workflowService;
        _env = env;
    }

    [HttpPost("run-test")]
    public async Task<IActionResult> RunTest()
    {
        if (!_env.IsDevelopment())
        {
            return NotFound();
        }

        try
        {
            // 0. Get a user (Ensure at least one user exists, usually Admin from seeding)
            var user = await _context.WebUsers.FirstOrDefaultAsync();
            if (user == null)
            {
                return BadRequest("No users found in database. Please seed users first.");
            }

            // Adım 1: Kod üzerinden yeni bir Process ("Test Süreci") ve Steps ("Başvuru", "Onay") oluşturup kaydet.
            var processCode = "TEST_" + DateTime.Now.Ticks;
            var process = new Process
            {
                Id = Guid.NewGuid(),
                Name = "Test Süreci",
                Code = processCode,
                Version = 1,
                Description = "Otomatik oluşturulan test süreci",
                CreatedBy = user.Username,
                CreatedAt = DateTime.UtcNow
            };

            var stepBasvuru = new ProcessStep
            {
                Id = Guid.NewGuid(),
                Name = "Başvuru",
                StepType = ProcessStepType.Start,
                OrderIndex = 1,
                ProcessId = process.Id,
                CreatedBy = user.Username,
                CreatedAt = DateTime.UtcNow
            };

            var stepOnay = new ProcessStep
            {
                Id = Guid.NewGuid(),
                Name = "Onay",
                StepType = ProcessStepType.UserTask,
                OrderIndex = 2,
                ProcessId = process.Id,
                CreatedBy = user.Username,
                CreatedAt = DateTime.UtcNow
            };

            var actionGecis = new ProcessAction
            {
                Id = Guid.NewGuid(),
                Name = "Onaya Gönder",
                ActionType = ProcessActionType.Approve,
                ProcessStepId = stepBasvuru.Id,
                TargetStepId = stepOnay.Id,
                CreatedBy = user.Username,
                CreatedAt = DateTime.UtcNow
            };

            stepBasvuru.Actions.Add(actionGecis);
            process.Steps.Add(stepBasvuru);
            process.Steps.Add(stepOnay);

            _context.Processes.Add(process);
            await _context.SaveChangesAsync();

            // Adım 2: WorkflowService.StartProcess ile bu süreci başlat.
            var requestId = await _workflowService.StartProcessAsync(process.Code, user.Id);

            // Adım 3: WorkflowService.ExecuteAction ile "Başvuru" adımından "Onay" adımına geçiş yap.
            var executeDto = new ExecuteActionDto
            {
                RequestId = requestId,
                ActionName = "Onaya Gönder",
                UserId = user.Id,
                Comments = "Test geçişi yapıldı"
            };

            await _workflowService.ExecuteActionAsync(executeDto);

            // Adım 4: ProcessRequestHistory tablosunu sorgula ve tarihçenin oluştuğunu doğrula.
            var history = await _context.ProcessRequestHistories
                .Where(h => h.ProcessRequestId == requestId)
                .OrderBy(h => h.CreatedAt)
                .ToListAsync();

            if (!history.Any())
            {
                throw new Exception("History not created!");
            }

            // Return success with details
            return Ok(new
            {
                Success = true,
                Message = "Smoke Test Passed",
                Data = new
                {
                    ProcessId = process.Id,
                    ProcessCode = process.Code,
                    RequestId = requestId,
                    HistoryCount = history.Count,
                    HistoryIds = history.Select(h => h.Id).ToList()
                }
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                Success = false,
                Message = ex.Message,
                StackTrace = ex.StackTrace
            });
        }
    }
}

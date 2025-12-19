using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using WorkflowEngine.Core.Interfaces;
using WorkflowEngine.Infrastructure.Hubs;

namespace WorkflowEngine.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(IHubContext<NotificationHub> hubContext, ILogger<NotificationService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task SendNotificationAsync(Guid userId, string message)
    {
        await _hubContext.Clients.User(userId.ToString()).SendAsync("ReceiveNotification", message);
        _logger.LogInformation("Sent notification to User {UserId}: {Message}", userId, message);
    }

    public async Task SendUpdateAsync(Guid userId)
    {
        await _hubContext.Clients.User(userId.ToString()).SendAsync("ReceiveUpdate");
    }

    public async Task SendUpdateToAllAsync()
    {
        await _hubContext.Clients.All.SendAsync("ReceiveUpdate");
    }
}

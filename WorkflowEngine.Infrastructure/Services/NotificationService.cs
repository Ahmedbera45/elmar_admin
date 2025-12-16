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
        // 1. Send via SignalR
        // Assuming userId is used as the UserIdentifier in SignalR (mapped from Claims)
        await _hubContext.Clients.User(userId.ToString()).SendAsync("ReceiveNotification", message);

        // 2. Simulate Email (Log)
        _logger.LogInformation("Simulating Email to User {UserId}: {Message}", userId, message);
    }
}

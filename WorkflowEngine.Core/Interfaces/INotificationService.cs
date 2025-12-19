using System;
using System.Threading.Tasks;

namespace WorkflowEngine.Core.Interfaces;

public interface INotificationService
{
    Task SendNotificationAsync(Guid userId, string message);
    Task SendUpdateAsync(Guid userId);
    Task SendUpdateToAllAsync();
}

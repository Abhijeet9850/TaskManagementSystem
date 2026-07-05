using TaskManager.API.Models;

namespace TaskManager.API.Services
{
    public interface INotificationService
    {
        Task NotifyTaskAssignedAsync(TaskItem task);
        Task NotifyTaskCompletedAsync(TaskItem task);
        Task CheckAndNotifyDueSoonAsync();
    }

}

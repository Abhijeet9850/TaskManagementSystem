using Microsoft.EntityFrameworkCore;
using TaskManager.API.Data;
using TaskManager.API.Models;

namespace TaskManager.API.Services
{
    // NOTE: This creates in-app notification records.
    // as needed for real-time or external delivery.
    public class NotificationService : INotificationService
    {
        private readonly AppDbContext _db;

        public NotificationService(AppDbContext db)
        {
            _db = db;
        }

        private async Task<int?> GetUserIdForEmployeeAsync(int employeeId)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.EmployeeId == employeeId);
            return user?.Id;
        }

        public async Task NotifyTaskAssignedAsync(TaskItem task)
        {
            var userId = await GetUserIdForEmployeeAsync(task.AssignedEmployeeId);
            if (userId is null) return;

            _db.Notifications.Add(new Notification
            {
                UserId = userId.Value,
                Message = $"You have been assigned a new task: \"{task.Title}\" (Due: {task.DueDate:d}).",
                Type = NotificationType.TaskAssigned,
                TaskId = task.Id
            });
            await _db.SaveChangesAsync();
        }

        public async Task NotifyTaskCompletedAsync(TaskItem task)
        {
            var userId = await GetUserIdForEmployeeAsync(task.AssignedEmployeeId);
            if (userId is null) return;

            _db.Notifications.Add(new Notification
            {
                UserId = userId.Value,
                Message = $"Task \"{task.Title}\" has been marked complete.",
                Type = NotificationType.TaskCompleted,
                TaskId = task.Id
            });
            await _db.SaveChangesAsync();
        }

        // Intended to be invoked by a scheduled/background job (e.g. hosted service, Hangfire, cron)
        // to notify employees whose tasks are due within the next 24 hours.
        public async Task CheckAndNotifyDueSoonAsync()
        {
            var cutoff = DateTime.UtcNow.AddDays(1);

            var dueSoonTasks = await _db.Tasks
                .Where(t => t.Status != TaskItemStatus.Completed && t.DueDate <= cutoff && t.DueDate >= DateTime.UtcNow)
                .ToListAsync();

            foreach (var task in dueSoonTasks)
            {
                var userId = await GetUserIdForEmployeeAsync(task.AssignedEmployeeId);
                if (userId is null) continue;

                bool alreadyNotified = await _db.Notifications.AnyAsync(n =>
                    n.TaskId == task.Id && n.Type == NotificationType.TaskDueSoon);

                if (alreadyNotified) continue;

                _db.Notifications.Add(new Notification
                {
                    UserId = userId.Value,
                    Message = $"Task \"{task.Title}\" is due within 24 hours (Due: {task.DueDate:d}).",
                    Type = NotificationType.TaskDueSoon,
                    TaskId = task.Id
                });
            }

            await _db.SaveChangesAsync();
        }
    }

}

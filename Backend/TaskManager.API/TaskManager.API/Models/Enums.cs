namespace TaskManager.API.Models
{
    public enum UserRole
    {
        Admin = 1,
        Employee = 2
    }

    public enum TaskPriority
    {
        Low = 1,
        Medium = 2,
        High = 3
    }

    public enum TaskItemStatus
    {
        Pending = 1,
        InProgress = 2,
        Completed = 3
    }

    public enum NotificationType
    {
        TaskAssigned = 1,
        TaskDueSoon = 2,
        TaskCompleted = 3
    }
}

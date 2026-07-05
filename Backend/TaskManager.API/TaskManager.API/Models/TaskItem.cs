namespace TaskManager.API.Models
{
    public class TaskItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public TaskPriority Priority { get; set; }
        public TaskItemStatus Status { get; set; } = TaskItemStatus.Pending;
        public DateTime StartDate { get; set; }
        public DateTime DueDate { get; set; }

        public int AssignedEmployeeId { get; set; }
        public Employee? AssignedEmployee { get; set; }

        public ICollection<TaskAttachment> Attachments { get; set; } = new List<TaskAttachment>();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
    }
}

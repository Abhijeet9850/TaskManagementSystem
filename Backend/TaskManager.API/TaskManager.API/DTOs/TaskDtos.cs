using System.ComponentModel.DataAnnotations;
using TaskManager.API.Models;

namespace TaskManager.API.DTOs
{
    // Used with [FromForm] so an optional initial attachment can be included on create.
    public class TaskCreateDto
    {
        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Required]
        public TaskPriority Priority { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime DueDate { get; set; }

        [Required]
        public int AssignedEmployeeId { get; set; }
    }

    public class TaskUpdateDto
    {
        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Required]
        public TaskPriority Priority { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime DueDate { get; set; }

        [Required]
        public int AssignedEmployeeId { get; set; }

        [Required]
        public TaskItemStatus Status { get; set; }
    }

    public class TaskAttachmentDto
    {
        public int Id { get; set; }
        public string OriginalFileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long FileSizeBytes { get; set; }
        public bool IsMain { get; set; }
        public DateTime UploadedAt { get; set; }
        public string UploadedByRole { get; set; } = string.Empty;
    }

    public class TaskResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime DueDate { get; set; }
        public int AssignedEmployeeId { get; set; }
        public string AssignedEmployeeName { get; set; } = string.Empty;
        public bool IsOverdue { get; set; }
        public List<TaskAttachmentDto> Attachments { get; set; } = new();
    }
}

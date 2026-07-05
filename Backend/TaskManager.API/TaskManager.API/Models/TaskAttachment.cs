using DocumentFormat.OpenXml.Spreadsheet;

namespace TaskManager.API.Models
{
    public class TaskAttachment
    {
        public int Id { get; set; }

        public int TaskId { get; set; }
        public TaskItem? Task { get; set; }

        public string FilePath { get; set; } = string.Empty;        // relative path on disk
        public string OriginalFileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long FileSizeBytes { get; set; }

        // The "main" attachment is the one set at task creation / replaced by an admin.
        // Non-main attachments are "supporting documents" uploaded by the assigned employee.
        public bool IsMain { get; set; }

        public int UploadedByUserId { get; set; }
        public User? UploadedByUser { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}

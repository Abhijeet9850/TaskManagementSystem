using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManager.API.Data;
using TaskManager.API.DTOs;
using TaskManager.API.Models;
using TaskManager.API.Services;

namespace TaskManager.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TasksController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly INotificationService _notificationService;
        private readonly IFileService _fileService;

        public TasksController(AppDbContext db, INotificationService notificationService, IFileService fileService)
        {
            _db = db;
            _notificationService = notificationService;
            _fileService = fileService;
        }

        private bool IsAdmin => User.IsInRole("Admin");

        private int CurrentUserId =>
            int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("sub")!.Value);

        private int? CurrentEmployeeId
        {
            get
            {
                var claim = User.FindFirst("employeeId")?.Value;
                return int.TryParse(claim, out var id) ? id : null;
            }
        }

        private static TaskResponseDto ToDto(TaskItem t) => new()
        {
            Id = t.Id,
            Title = t.Title,
            Description = t.Description,
            Priority = t.Priority.ToString(),
            Status = t.Status.ToString(),
            StartDate = t.StartDate,
            DueDate = t.DueDate,
            AssignedEmployeeId = t.AssignedEmployeeId,
            AssignedEmployeeName = t.AssignedEmployee?.Name ?? string.Empty,
            IsOverdue = t.Status != TaskItemStatus.Completed && t.DueDate.Date < DateTime.UtcNow.Date,
            Attachments = t.Attachments
                .OrderByDescending(a => a.IsMain)
                .ThenByDescending(a => a.UploadedAt)
                .Select(a => new TaskAttachmentDto
                {
                    Id = a.Id,
                    OriginalFileName = a.OriginalFileName,
                    ContentType = a.ContentType,
                    FileSizeBytes = a.FileSizeBytes,
                    IsMain = a.IsMain,
                    UploadedAt = a.UploadedAt,
                    UploadedByRole = a.UploadedByUser?.Role.ToString() ?? string.Empty
                }).ToList()
        };

        // Checks whether the current user is allowed to see/act on this specific task at all.
        private bool CanAccessTask(TaskItem task) => IsAdmin || task.AssignedEmployeeId == CurrentEmployeeId;

        // ---------- Task CRUD ----------

        // GET api/tasks
        // Employees only see their own tasks; admins see all tasks.
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskResponseDto>>> GetAll()
        {
            var query = _db.Tasks
                .Include(t => t.AssignedEmployee)
                .Include(t => t.Attachments).ThenInclude(a => a.UploadedByUser)
                .AsQueryable();

            if (!IsAdmin)
            {
                if (CurrentEmployeeId is null) return Ok(Enumerable.Empty<TaskResponseDto>());
                query = query.Where(t => t.AssignedEmployeeId == CurrentEmployeeId);
            }

            var tasks = await query.OrderByDescending(t => t.CreatedAt).ToListAsync();
            return Ok(tasks.Select(ToDto));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TaskResponseDto>> GetById(int id)
        {
            var task = await _db.Tasks
                .Include(t => t.AssignedEmployee)
                .Include(t => t.Attachments).ThenInclude(a => a.UploadedByUser)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null) return NotFound();
            if (!CanAccessTask(task)) return Forbid();

            return Ok(ToDto(task));
        }

        // POST api/tasks (multipart/form-data)
        // Admin only. An initial attachment can optionally be uploaded at the same time.
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromForm] TaskCreateDto dto, IFormFile? file)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (dto.DueDate.Date < dto.StartDate.Date)
                return BadRequest(new { message = "Due Date must not be earlier than Start Date." });

            var employeeExists = await _db.Employees.AnyAsync(e => e.Id == dto.AssignedEmployeeId);
            if (!employeeExists) return BadRequest(new { message = "Assigned employee does not exist." });

            var task = new TaskItem
            {
                Title = dto.Title,
                Description = dto.Description,
                Priority = dto.Priority,
                StartDate = dto.StartDate,
                DueDate = dto.DueDate,
                AssignedEmployeeId = dto.AssignedEmployeeId,
                Status = TaskItemStatus.Pending
            };

            _db.Tasks.Add(task);
            await _db.SaveChangesAsync();

            if (file != null)
            {
                var (success, message, filePath, originalName, size, contentType) = await _fileService.SaveFileAsync(file);
                if (!success) return BadRequest(new { message });

                _db.TaskAttachments.Add(new TaskAttachment
                {
                    TaskId = task.Id,
                    FilePath = filePath!,
                    OriginalFileName = originalName!,
                    ContentType = contentType!,
                    FileSizeBytes = size,
                    IsMain = true,
                    UploadedByUserId = CurrentUserId
                });
                await _db.SaveChangesAsync();
            }

            await _notificationService.NotifyTaskAssignedAsync(task);

            var created = await _db.Tasks
                .Include(t => t.AssignedEmployee)
                .Include(t => t.Attachments).ThenInclude(a => a.UploadedByUser)
                .FirstAsync(t => t.Id == task.Id);

            return CreatedAtAction(nameof(GetById), new { id = task.Id }, ToDto(created));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, TaskUpdateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var task = await _db.Tasks.Include(t => t.AssignedEmployee).FirstOrDefaultAsync(t => t.Id == id);
            if (task == null) return NotFound();
            if (!CanAccessTask(task)) return Forbid();

            // Completed tasks cannot be edited.
            if (task.Status == TaskItemStatus.Completed)
                return BadRequest(new { message = "Completed tasks cannot be edited." });

            if (dto.DueDate.Date < dto.StartDate.Date)
                return BadRequest(new { message = "Due Date must not be earlier than Start Date." });

            if (IsAdmin)
            {
                var employeeExists = await _db.Employees.AnyAsync(e => e.Id == dto.AssignedEmployeeId);
                if (!employeeExists) return BadRequest(new { message = "Assigned employee does not exist." });
                task.AssignedEmployeeId = dto.AssignedEmployeeId;
            }

            bool justCompleted = dto.Status == TaskItemStatus.Completed && task.Status != TaskItemStatus.Completed;

            task.Title = dto.Title;
            task.Description = dto.Description;
            task.Priority = dto.Priority;
            task.StartDate = dto.StartDate;
            task.DueDate = dto.DueDate;
            task.Status = dto.Status;

            if (justCompleted)
                task.CompletedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            if (justCompleted)
                await _notificationService.NotifyTaskCompletedAsync(task);

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var task = await _db.Tasks.Include(t => t.Attachments).FirstOrDefaultAsync(t => t.Id == id);
            if (task == null) return NotFound();

            foreach (var attachment in task.Attachments)
                _fileService.DeletePhysicalFile(attachment.FilePath);

            _db.Tasks.Remove(task);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        // ---------- Attachments ----------

        // GET api/tasks/{id}/attachments
        [HttpGet("{id}/attachments")]
        public async Task<ActionResult<IEnumerable<TaskAttachmentDto>>> GetAttachments(int id)
        {
            var task = await _db.Tasks
                .Include(t => t.Attachments).ThenInclude(a => a.UploadedByUser)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null) return NotFound();
            if (!CanAccessTask(task)) return Forbid();

            return Ok(ToDto(task).Attachments);
        }

        // POST api/tasks/{id}/attachments — upload a supporting document.
        // Employees may add supporting docs to their own assigned tasks; admins may add to any task.
        [HttpPost("{id}/attachments")]
        public async Task<IActionResult> UploadSupportingDocument(int id, IFormFile file)
        {
            var task = await _db.Tasks.FindAsync(id);
            if (task == null) return NotFound();
            if (!CanAccessTask(task)) return Forbid();

            var (success, message, filePath, originalName, size, contentType) = await _fileService.SaveFileAsync(file);
            if (!success) return BadRequest(new { message });

            var attachment = new TaskAttachment
            {
                TaskId = id,
                FilePath = filePath!,
                OriginalFileName = originalName!,
                ContentType = contentType!,
                FileSizeBytes = size,
                IsMain = false,
                UploadedByUserId = CurrentUserId
            };

            _db.TaskAttachments.Add(attachment);
            await _db.SaveChangesAsync();

            return Ok(new TaskAttachmentDto
            {
                Id = attachment.Id,
                OriginalFileName = attachment.OriginalFileName,
                ContentType = attachment.ContentType,
                FileSizeBytes = attachment.FileSizeBytes,
                IsMain = attachment.IsMain,
                UploadedAt = attachment.UploadedAt,
                UploadedByRole = IsAdmin ? "Admin" : "Employee"
            });
        }

        // PUT api/tasks/{id}/attachment — Admin-only: replace the main attachment.
        // Only allowed before the task is marked Completed.
        [HttpPut("{id}/attachment")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ReplaceMainAttachment(int id, IFormFile file)
        {
            var task = await _db.Tasks.Include(t => t.Attachments).FirstOrDefaultAsync(t => t.Id == id);
            if (task == null) return NotFound();

            if (task.Status == TaskItemStatus.Completed)
                return BadRequest(new { message = "Attachments cannot be replaced on a completed task." });

            var (success, message, filePath, originalName, size, contentType) = await _fileService.SaveFileAsync(file);
            if (!success) return BadRequest(new { message });

            var existingMain = task.Attachments.FirstOrDefault(a => a.IsMain);
            if (existingMain != null)
            {
                _fileService.DeletePhysicalFile(existingMain.FilePath);
                _db.TaskAttachments.Remove(existingMain);
            }

            _db.TaskAttachments.Add(new TaskAttachment
            {
                TaskId = id,
                FilePath = filePath!,
                OriginalFileName = originalName!,
                ContentType = contentType!,
                FileSizeBytes = size,
                IsMain = true,
                UploadedByUserId = CurrentUserId
            });

            await _db.SaveChangesAsync();
            return Ok(new { message = "Attachment replaced successfully." });
        }

        // GET api/tasks/attachments/{attachmentId}/download
        // Admin can download any employee's attachments; employees can only download
        // attachments on tasks assigned to them.
        [HttpGet("attachments/{attachmentId}/download")]
        public async Task<IActionResult> DownloadAttachment(int attachmentId)
        {
            var attachment = await _db.TaskAttachments.Include(a => a.Task).FirstOrDefaultAsync(a => a.Id == attachmentId);
            if (attachment == null) return NotFound();

            if (!IsAdmin && attachment.Task!.AssignedEmployeeId != CurrentEmployeeId)
                return Forbid();

            var physicalPath = _fileService.GetPhysicalPath(attachment.FilePath);
            if (physicalPath == null) return NotFound(new { message = "File is missing from storage." });

            var bytes = await System.IO.File.ReadAllBytesAsync(physicalPath);
            return File(bytes, attachment.ContentType, attachment.OriginalFileName);
        }

        // DELETE api/tasks/attachments/{attachmentId} — Admin only.
        [HttpDelete("attachments/{attachmentId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteAttachment(int attachmentId)
        {
            var attachment = await _db.TaskAttachments.FindAsync(attachmentId);
            if (attachment == null) return NotFound();

            _fileService.DeletePhysicalFile(attachment.FilePath);
            _db.TaskAttachments.Remove(attachment);
            await _db.SaveChangesAsync();

            return NoContent();
        }
    }

}

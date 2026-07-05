using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManager.API.Data;
using TaskManager.API.DTOs;

namespace TaskManager.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly AppDbContext _db;

        public NotificationsController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<NotificationResponseDto>>> GetMyNotifications()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("sub")?.Value;

            if (!int.TryParse(userIdClaim, out var userId))
                return Ok(Enumerable.Empty<NotificationResponseDto>());

            var notifications = await _db.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new NotificationResponseDto
                {
                    Id = n.Id,
                    Message = n.Message,
                    Type = n.Type.ToString(),
                    IsRead = n.IsRead,
                    CreatedAt = n.CreatedAt,
                    TaskId = n.TaskId
                })
                .ToListAsync();

            return Ok(notifications);
        }

        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var notification = await _db.Notifications.FindAsync(id);
            if (notification == null) return NotFound();

            notification.IsRead = true;
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}

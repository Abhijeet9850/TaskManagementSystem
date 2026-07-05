using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManager.API.Data;
using TaskManager.API.DTOs;
using TaskManager.API.Models;

namespace TaskManager.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly AppDbContext _db;

        public DashboardController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<AdminDashboardDto>> GetAdminDashboard()
        {
            var totalEmployees = await _db.Employees.CountAsync();
            var totalTasks = await _db.Tasks.CountAsync();
            var completedTasks = await _db.Tasks.CountAsync(t => t.Status == TaskItemStatus.Completed);
            var pendingTasks = await _db.Tasks.CountAsync(t => t.Status != TaskItemStatus.Completed);

            return Ok(new AdminDashboardDto
            {
                TotalEmployees = totalEmployees,
                TotalTasks = totalTasks,
                CompletedTasks = completedTasks,
                PendingTasks = pendingTasks
            });
        }

        [HttpGet("employee")]
        public async Task<ActionResult<EmployeeDashboardDto>> GetEmployeeDashboard()
        {
            var employeeIdClaim = User.FindFirst("employeeId")?.Value;
            if (!int.TryParse(employeeIdClaim, out var employeeId))
                return BadRequest(new { message = "No employee profile linked to this account." });

            var tasks = await _db.Tasks.Where(t => t.AssignedEmployeeId == employeeId).ToListAsync();

            return Ok(new EmployeeDashboardDto
            {
                MyTasks = tasks.Count,
                CompletedTasks = tasks.Count(t => t.Status == TaskItemStatus.Completed),
                PendingTasks = tasks.Count(t => t.Status != TaskItemStatus.Completed),
                OverdueTasks = tasks.Count(t => t.Status != TaskItemStatus.Completed && t.DueDate.Date < DateTime.UtcNow.Date)
            });
        }
    }
}

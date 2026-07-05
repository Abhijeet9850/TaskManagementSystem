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
    [Authorize(Roles = "Admin")]
    public class EmployeesController : ControllerBase
    {
        private readonly AppDbContext _db;

        public EmployeesController(AppDbContext db)
        {
            _db = db;
        }

        // GET api/employees?search=&sortBy=name&sortDir=asc&page=1&pageSize=10
        [HttpGet]
        public async Task<ActionResult<PagedResult<EmployeeResponseDto>>> GetAll(
            [FromQuery] string? search,
            [FromQuery] string sortBy = "name",
            [FromQuery] string sortDir = "asc",
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = _db.Employees.Include(e => e.Tasks).AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                query = query.Where(e =>
                    e.Name.ToLower().Contains(s) ||
                    e.Email.ToLower().Contains(s) ||
                    e.Department.ToLower().Contains(s) ||
                    e.Designation.ToLower().Contains(s));
            }

            query = (sortBy.ToLower(), sortDir.ToLower()) switch
            {
                ("name", "desc") => query.OrderByDescending(e => e.Name),
                ("name", _) => query.OrderBy(e => e.Name),
                ("department", "desc") => query.OrderByDescending(e => e.Department),
                ("department", _) => query.OrderBy(e => e.Department),
                ("designation", "desc") => query.OrderByDescending(e => e.Designation),
                ("designation", _) => query.OrderBy(e => e.Designation),
                _ => query.OrderBy(e => e.Name)
            };

            var totalCount = await query.CountAsync();

            var employees = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var employeeIds = employees.Select(e => e.Id).ToList();
            var linkedEmails = await _db.Users
                .Where(u => u.EmployeeId != null && employeeIds.Contains(u.EmployeeId.Value))
                .ToDictionaryAsync(u => u.EmployeeId!.Value, u => u.Email);

            var items = employees.Select(e => new EmployeeResponseDto
            {
                Id = e.Id,
                Name = e.Name,
                Email = e.Email,
                Department = e.Department,
                Designation = e.Designation,
                TotalTasks = e.Tasks.Count,
                CompletedTasks = e.Tasks.Count(t => t.Status == TaskItemStatus.Completed),
                LinkedUserEmail = linkedEmails.GetValueOrDefault(e.Id)
            }).ToList();

            return Ok(new PagedResult<EmployeeResponseDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = pageSize
            });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EmployeeResponseDto>> GetById(int id)
        {
            var e = await _db.Employees.Include(x => x.Tasks).FirstOrDefaultAsync(x => x.Id == id);
            if (e == null) return NotFound();

            var linkedUser = await _db.Users.FirstOrDefaultAsync(u => u.EmployeeId == id);

            return Ok(new EmployeeResponseDto
            {
                Id = e.Id,
                Name = e.Name,
                Email = e.Email,
                Department = e.Department,
                Designation = e.Designation,
                TotalTasks = e.Tasks.Count,
                CompletedTasks = e.Tasks.Count(t => t.Status == TaskItemStatus.Completed),
                LinkedUserEmail = linkedUser?.Email
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create(EmployeeCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var emailExists = await _db.Employees.AnyAsync(e => e.Email.ToLower() == dto.Email.ToLower());
            if (emailExists) return Conflict(new { message = "An employee with this email already exists." });

            var employee = new Employee
            {
                Name = dto.Name,
                Email = dto.Email,
                Department = dto.Department,
                Designation = dto.Designation
            };

            _db.Employees.Add(employee);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = employee.Id }, employee);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, EmployeeUpdateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var employee = await _db.Employees.FindAsync(id);
            if (employee == null) return NotFound();

            var emailTaken = await _db.Employees.AnyAsync(e => e.Id != id && e.Email.ToLower() == dto.Email.ToLower());
            if (emailTaken) return Conflict(new { message = "Another employee already uses this email." });

            employee.Name = dto.Name;
            employee.Email = dto.Email;
            employee.Department = dto.Department;
            employee.Designation = dto.Designation;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var employee = await _db.Employees.FindAsync(id);
            if (employee == null) return NotFound();

            _db.Employees.Remove(employee);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        // PUT api/employees/{id}/link-user
        // Links this Employee (HR) record to an existing login account by email,
        // so that account's JWT gets the employeeId claim needed to see their tasks.
        [HttpPut("{id}/link-user")]
        public async Task<IActionResult> LinkUser(int id, LinkUserDto dto)
        {
            var employee = await _db.Employees.FindAsync(id);
            if (employee == null) return NotFound(new { message = "Employee not found." });

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == dto.Email.ToLower());
            if (user == null)
                return NotFound(new { message = "No login account found with that email. Ask them to register first." });

            if (user.Role != UserRole.Employee)
                return BadRequest(new { message = "Only Employee-role login accounts can be linked to an employee record." });

            var alreadyLinkedElsewhere = await _db.Users.AnyAsync(u => u.EmployeeId == id && u.Id != user.Id);
            if (alreadyLinkedElsewhere)
                return Conflict(new { message = "This employee record is already linked to a different login account." });

            user.EmployeeId = id;
            await _db.SaveChangesAsync();

            return Ok(new { message = $"Linked {user.Email} to employee \"{employee.Name}\". They'll see their tasks next time they log in." });
        }

        // PUT api/employees/{id}/unlink-user
        [HttpPut("{id}/unlink-user")]
        public async Task<IActionResult> UnlinkUser(int id)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.EmployeeId == id);
            if (user == null) return NotFound(new { message = "No login account is linked to this employee." });

            user.EmployeeId = null;
            await _db.SaveChangesAsync();

            return Ok(new { message = "Login account unlinked." });
        }
    }

}

using System.ComponentModel.DataAnnotations;

namespace TaskManager.API.DTOs
{
    public class EmployeeCreateDto
    {
        [Required, MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string Department { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string Designation { get; set; } = string.Empty;
    }

    public class EmployeeUpdateDto : EmployeeCreateDto
    {
    }

    // Links an existing Employee record (HR profile) to a User's login account,
    // by the login account's email — the two are independent tables, so an
    // employee sees no tasks until this link is made.
    public class LinkUserDto
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;
    }

    public class EmployeeResponseDto
    {
        public int Id { get; set; }
        
        public string Name { get; set; } = string.Empty;
        
        public string Email { get; set; } = string.Empty;
        
        public string Department { get; set; } = string.Empty;
        
        public string Designation { get; set; } = string.Empty;
        
        public int TotalTasks { get; set; }
        
        public int CompletedTasks { get; set; }
        
        public string? LinkedUserEmail { get; set; }

    }

    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    }
}

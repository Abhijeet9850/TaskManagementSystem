using Microsoft.EntityFrameworkCore;
using TaskManager.API.Models;

namespace TaskManager.API.Data
{
    public class AppDbContext: DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Employee> Employees => Set<Employee>();
        public DbSet<TaskItem> Tasks => Set<TaskItem>();
        public DbSet<Notification> Notifications => Set<Notification>();
        public DbSet<TaskAttachment> TaskAttachments => Set<TaskAttachment>();


    }
}

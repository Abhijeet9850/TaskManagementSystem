namespace TaskManager.API.DTOs
{
    public class AdminDashboardDto
    {
        public int TotalEmployees { get; set; }
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int PendingTasks { get; set; }
    }

    public class EmployeeDashboardDto
    {
        public int MyTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int PendingTasks { get; set; }
        public int OverdueTasks { get; set; }
    }
}

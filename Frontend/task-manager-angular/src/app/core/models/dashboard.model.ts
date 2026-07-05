export interface AdminDashboard {
  totalEmployees: number;
  totalTasks: number;
  completedTasks: number;
  pendingTasks: number;
}

export interface EmployeeDashboard {
  myTasks: number;
  completedTasks: number;
  pendingTasks: number;
  overdueTasks: number;
}

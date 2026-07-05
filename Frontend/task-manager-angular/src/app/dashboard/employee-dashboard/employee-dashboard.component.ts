import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DashboardService } from '../../core/services/dashboard.service';
import { AuthService } from '../../core/services/auth.service';
import { EmployeeDashboard } from '../../core/models/dashboard.model';

@Component({
  selector: 'app-employee-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './employee-dashboard.component.html'
})
export class EmployeeDashboardComponent implements OnInit {
  stats: EmployeeDashboard | null = null;
  errorMessage = '';

  constructor(private dashboardService: DashboardService, public auth: AuthService) {}

  ngOnInit(): void {
    this.dashboardService.getEmployeeDashboard().subscribe({
      next: (data) => this.stats = data,
      error: (err) => {
        console.error('Failed to load employee dashboard:', err);
        this.errorMessage = err.status === 0
          ? 'Cannot reach the API. Check the backend is running and the port in core/api-config.ts matches.'
          : (err.error?.message || 'Failed to load dashboard data.');
      }
    });
  }

  get completionPercent(): number {
    if (!this.stats || this.stats.myTasks === 0) return 0;
    return Math.round((this.stats.completedTasks / this.stats.myTasks) * 100);
  }

  get greeting(): string {
    const hour = new Date().getHours();
    if (hour < 12) return 'Good morning';
    if (hour < 17) return 'Good afternoon';
    return 'Good evening';
  }
}

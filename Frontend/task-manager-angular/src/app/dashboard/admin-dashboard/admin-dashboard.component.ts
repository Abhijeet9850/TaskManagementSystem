import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { DashboardService } from '../../core/services/dashboard.service';
import { AdminDashboard } from '../../core/models/dashboard.model';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './admin-dashboard.component.html'
})
export class AdminDashboardComponent implements OnInit {
  stats: AdminDashboard | null = null;
  errorMessage = '';

  constructor(private dashboardService: DashboardService) {}

  ngOnInit(): void {
    this.dashboardService.getAdminDashboard().subscribe({
      next: (data) => this.stats = data,
      error: (err) => {
        console.error('Failed to load admin dashboard:', err);
        this.errorMessage = err.status === 0
          ? 'Cannot reach the API. Check the backend is running and the port in core/api-config.ts matches.'
          : (err.error?.message || 'Failed to load dashboard data.');
      }
    });
  }
}

import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AuthService } from '../core/services/auth.service';
import { AdminDashboardComponent } from './admin-dashboard/admin-dashboard.component';
import { EmployeeDashboardComponent } from './employee-dashboard/employee-dashboard.component';

@Component({
  selector: 'app-dashboard-router',
  standalone: true,
  imports: [CommonModule, AdminDashboardComponent, EmployeeDashboardComponent],
  template: `
    <app-admin-dashboard *ngIf="auth.isAdmin(); else employeeView"></app-admin-dashboard>
    <ng-template #employeeView>
      <app-employee-dashboard></app-employee-dashboard>
    </ng-template>
  `
})
export class DashboardRouterComponent {
  constructor(public auth: AuthService) {}
}

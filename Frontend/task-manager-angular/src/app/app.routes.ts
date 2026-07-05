import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { adminGuard } from './core/guards/role.guard';
import { guestGuard, rootRedirectGuard } from './core/guards/guest.guard';

export const routes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    canActivate: [rootRedirectGuard],
    loadComponent: () => import('./auth/login/login.component').then(m => m.LoginComponent)
  },
  {
    path: 'login',
    canActivate: [guestGuard],
    loadComponent: () => import('./auth/login/login.component').then(m => m.LoginComponent)
  },
  {
    path: 'register',
    canActivate: [guestGuard],
    loadComponent: () => import('./auth/register/register.component').then(m => m.RegisterComponent)
  },
  {
    path: 'dashboard',
    canActivate: [authGuard],
    loadComponent: () => import('./dashboard/dashboard-router.component').then(m => m.DashboardRouterComponent)
  },
  {
    // Direct route for Admins — reached explicitly after login, not via role-check redirect.
    path: 'admin-dashboard',
    canActivate: [authGuard, adminGuard],
    loadComponent: () => import('./dashboard/admin-dashboard/admin-dashboard.component').then(m => m.AdminDashboardComponent)
  },
  {
    // Direct route for Employees — reached explicitly after login.
    path: 'employee-dashboard',
    canActivate: [authGuard],
    loadComponent: () => import('./dashboard/employee-dashboard/employee-dashboard.component').then(m => m.EmployeeDashboardComponent)
  },
  {
    path: 'employees',
    canActivate: [authGuard, adminGuard],
    loadComponent: () => import('./employees/employee-list/employee-list.component').then(m => m.EmployeeListComponent)
  },
  {
    path: 'tasks',
    canActivate: [authGuard],
    loadComponent: () => import('./tasks/task-list/task-list.component').then(m => m.TaskListComponent)
  },
  {
    path: 'reports',
    canActivate: [authGuard, adminGuard],
    loadComponent: () => import('./reports/reports/reports.component').then(m => m.ReportsComponent)
  },
  { path: '**', redirectTo: 'login' }
];

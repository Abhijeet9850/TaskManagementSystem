import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AdminDashboard, EmployeeDashboard } from '../models/dashboard.model';

import { API_BASE_URL } from '../api-config';
const API_URL = `${API_BASE_URL}/dashboard`;

@Injectable({ providedIn: 'root' })
export class DashboardService {
  constructor(private http: HttpClient) {}

  getAdminDashboard(): Observable<AdminDashboard> {
    return this.http.get<AdminDashboard>(`${API_URL}/admin`);
  }

  getEmployeeDashboard(): Observable<EmployeeDashboard> {
    return this.http.get<EmployeeDashboard>(`${API_URL}/employee`);
  }
}

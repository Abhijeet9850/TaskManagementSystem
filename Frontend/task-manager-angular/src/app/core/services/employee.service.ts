import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Employee, PagedResult } from '../models/employee.model';

import { API_BASE_URL } from '../api-config';
const API_URL = `${API_BASE_URL}/employees`;

@Injectable({ providedIn: 'root' })
export class EmployeeService {
  constructor(private http: HttpClient) {}

  getAll(search = '', sortBy = 'name', sortDir = 'asc', page = 1, pageSize = 10): Observable<PagedResult<Employee>> {
    const params = new HttpParams()
      .set('search', search)
      .set('sortBy', sortBy)
      .set('sortDir', sortDir)
      .set('page', page)
      .set('pageSize', pageSize);
    return this.http.get<PagedResult<Employee>>(API_URL, { params });
  }

  getById(id: number): Observable<Employee> {
    return this.http.get<Employee>(`${API_URL}/${id}`);
  }

  create(employee: Partial<Employee>): Observable<Employee> {
    return this.http.post<Employee>(API_URL, employee);
  }

  update(id: number, employee: Partial<Employee>): Observable<void> {
    return this.http.put<void>(`${API_URL}/${id}`, employee);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${API_URL}/${id}`);
  }

  // Links an existing login account (by email) to this Employee HR record.
  // The linked user must log out/in again afterward to get a fresh JWT with the employeeId claim.
  linkUser(employeeId: number, email: string): Observable<{ message: string }> {
    return this.http.put<{ message: string }>(`${API_URL}/${employeeId}/link-user`, { email });
  }

  unlinkUser(employeeId: number): Observable<{ message: string }> {
    return this.http.put<{ message: string }>(`${API_URL}/${employeeId}/unlink-user`, {});
  }
}

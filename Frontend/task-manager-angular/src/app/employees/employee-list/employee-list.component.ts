import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { debounceTime } from 'rxjs';
import { EmployeeService } from '../../core/services/employee.service';
import { Employee } from '../../core/models/employee.model';
import { EmployeeFormComponent } from '../employee-form/employee-form.component';

@Component({
  selector: 'app-employee-list',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, EmployeeFormComponent],
  templateUrl: './employee-list.component.html'
})
export class EmployeeListComponent implements OnInit {
  employees: Employee[] = [];
  searchControl = new FormControl('', { nonNullable: true });
  get search(): string {
    return this.searchControl.value;
  }
  sortBy = 'name';
  sortDir: 'asc' | 'desc' = 'asc';
  page = 1;
  pageSize = 10;
  totalPages = 1;
  totalCount = 0;

  showForm = false;
  editingEmployee: Employee | null = null;

  errorMessage = '';

  // Link-login inline panel state
  linkingEmployeeId: number | null = null;
  linkEmailControl = new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.email] });
  linkMessage = '';
  linkError = '';

  constructor(private employeeService: EmployeeService) {}

  ngOnInit(): void {
    this.loadEmployees();
    this.searchControl.valueChanges.pipe(debounceTime(300)).subscribe(() => this.onSearchChange());
  }

  loadEmployees(): void {
    this.errorMessage = '';
    this.employeeService.getAll(this.search, this.sortBy, this.sortDir, this.page, this.pageSize)
      .subscribe({
        next: (result) => {
          this.employees = result.items;
          this.totalPages = result.totalPages;
          this.totalCount = result.totalCount;
        },
        error: (err) => {
          console.error('Failed to load employees:', err);
          this.errorMessage = err.status === 0
            ? 'Cannot reach the API. Check the backend is running and the port in core/api-config.ts matches.'
            : (err.error?.message || 'Failed to load employees.');
        }
      });
  }

  onSearchChange(): void {
    this.page = 1;
    this.loadEmployees();
  }

  sort(column: string): void {
    if (this.sortBy === column) {
      this.sortDir = this.sortDir === 'asc' ? 'desc' : 'asc';
    } else {
      this.sortBy = column;
      this.sortDir = 'asc';
    }
    this.loadEmployees();
  }

  goToPage(p: number): void {
    if (p < 1 || p > this.totalPages) return;
    this.page = p;
    this.loadEmployees();
  }

  openAddForm(): void {
    this.editingEmployee = null;
    this.showForm = true;
  }

  openEditForm(emp: Employee): void {
    this.editingEmployee = emp;
    this.showForm = true;
  }

  deleteEmployee(emp: Employee): void {
    if (!confirm(`Delete employee "${emp.name}"?`)) return;
    this.employeeService.delete(emp.id).subscribe(() => this.loadEmployees());
  }

  onFormClosed(saved: boolean): void {
    this.showForm = false;
    if (saved) this.loadEmployees();
  }

  // ---- Link login account ----

  openLinkForm(emp: Employee): void {
    this.linkingEmployeeId = emp.id;
    this.linkEmailControl.setValue('');
    this.linkMessage = '';
    this.linkError = '';
  }

  cancelLink(): void {
    this.linkingEmployeeId = null;
  }

  submitLink(emp: Employee): void {
    this.linkMessage = '';
    this.linkError = '';

    if (this.linkEmailControl.invalid) {
      this.linkEmailControl.markAsTouched();
      return;
    }

    this.employeeService.linkUser(emp.id, this.linkEmailControl.value).subscribe({
      next: (res) => {
        this.linkMessage = res.message;
        this.loadEmployees();
      },
      error: (err) => this.linkError = err.error?.message || 'Failed to link login account.'
    });
  }

  unlinkUser(emp: Employee): void {
    if (!confirm(`Unlink the login account from "${emp.name}"? They'll stop seeing their tasks until relinked.`)) return;

    this.employeeService.unlinkUser(emp.id).subscribe({
      next: () => this.loadEmployees(),
      error: (err) => alert(err.error?.message || 'Failed to unlink.')
    });
  }
}

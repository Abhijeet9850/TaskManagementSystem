import { Component, EventEmitter, Input, Output, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Observable } from 'rxjs';
import { EmployeeService } from '../../core/services/employee.service';
import { Employee } from '../../core/models/employee.model';

@Component({
  selector: 'app-employee-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './employee-form.component.html'
})
export class EmployeeFormComponent implements OnInit {
  @Input() employee: Employee | null = null;
  @Output() closed = new EventEmitter<boolean>();

  errorMessage = '';

  form: FormGroup;

  constructor(private fb: FormBuilder, private employeeService: EmployeeService) {
    this.form = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(150)]],
      email: ['', [Validators.required, Validators.email]],
      department: ['', [Validators.required, Validators.maxLength(100)]],
      designation: ['', [Validators.required, Validators.maxLength(100)]]
    });
  }

  get f() {
    return this.form.controls;
  }

  ngOnInit(): void {
    if (this.employee) {
      this.form.patchValue({
        name: this.employee.name,
        email: this.employee.email,
        department: this.employee.department,
        designation: this.employee.designation
      });
    }
  }

  get isEditMode(): boolean {
    return !!this.employee;
  }

  save(): void {
    this.errorMessage = '';

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const payload = this.form.getRawValue();

    const request$: Observable<unknown> = this.isEditMode
      ? this.employeeService.update(this.employee!.id, payload as Partial<Employee>)
      : this.employeeService.create(payload as Partial<Employee>);

    request$.subscribe({
      next: () => this.closed.emit(true),
      error: (err: any) => this.errorMessage = err.error?.message || 'Failed to save employee.'
    });
  }

  cancel(): void {
    this.closed.emit(false);
  }
}

import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Observable } from 'rxjs';
import { TaskService } from '../../core/services/task.service';
import { EmployeeService } from '../../core/services/employee.service';
import { AuthService } from '../../core/services/auth.service';
import { TaskItem, TaskPriority, TaskStatus } from '../../core/models/task.model';
import { Employee } from '../../core/models/employee.model';
import { dueDateNotBeforeStartValidator } from '../../core/validators/custom-validators';

@Component({
  selector: 'app-task-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './task-form.component.html'
})
export class TaskFormComponent implements OnInit {
  @Input() task: TaskItem | null = null;
  @Output() closed = new EventEmitter<boolean>();

  employees: Employee[] = [];
  errorMessage = '';
  selectedFile: File | null = null;
  fileSizeError = '';

  form: FormGroup;

  constructor(
    private fb: FormBuilder,
    private taskService: TaskService,
    private employeeService: EmployeeService,
    public auth: AuthService
  ) {
    this.form = this.fb.group({
      title: ['', [Validators.required, Validators.maxLength(200)]],
      description: [''],
      priority: ['Medium' as TaskPriority, [Validators.required]],
      status: ['Pending' as TaskStatus, [Validators.required]],
      startDate: ['', [Validators.required]],
      dueDate: ['', [Validators.required]],
      assignedEmployeeId: [null as number | null, [Validators.required]]
    }, {
      validators: [dueDateNotBeforeStartValidator('startDate', 'dueDate')]
    });
  }

  get f() {
    return this.form.controls;
  }

  ngOnInit(): void {
    this.employeeService.getAll('', 'name', 'asc', 1, 100).subscribe(res => this.employees = res.items);

    if (this.task) {
      this.form.patchValue({
        title: this.task.title,
        description: this.task.description,
        priority: this.task.priority,
        status: this.task.status,
        startDate: this.task.startDate.substring(0, 10),
        dueDate: this.task.dueDate.substring(0, 10),
        assignedEmployeeId: this.task.assignedEmployeeId
      });
    }
  }

  get isEditMode(): boolean {
    return !!this.task;
  }

  onFileSelected(event: Event): void {
    this.fileSizeError = '';
    const input = event.target as HTMLInputElement;
    if (!input.files?.length) {
      this.selectedFile = null;
      return;
    }

    const file = input.files[0];
    if (file.size > 5 * 1024 * 1024) {
      this.fileSizeError = 'File exceeds the 5 MB limit.';
      this.selectedFile = null;
      input.value = '';
      return;
    }

    this.selectedFile = file;
  }

  save(): void {
    this.errorMessage = '';

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      if (this.form.errors?.['dueBeforeStart']) {
        this.errorMessage = 'Due Date must not be earlier than Start Date.';
      }
      return;
    }

    const { title, description, priority, status, startDate, dueDate, assignedEmployeeId } = this.form.getRawValue();

    const basePayload = {
      title: title!,
      description: description ?? '',
      priority: priority!,
      startDate: startDate!,
      dueDate: dueDate!,
      assignedEmployeeId: assignedEmployeeId!
    };

    const request$: Observable<unknown> = this.isEditMode
      ? this.taskService.update(this.task!.id, { ...basePayload, status: status! })
      : this.taskService.create(basePayload, this.selectedFile);

    request$.subscribe({
      next: () => this.closed.emit(true),
      error: (err: any) => this.errorMessage = err.error?.message || 'Failed to save task.'
    });
  }

  cancel(): void {
    this.closed.emit(false);
  }
}

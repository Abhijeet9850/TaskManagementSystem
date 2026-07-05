import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TaskService } from '../../core/services/task.service';
import { AuthService } from '../../core/services/auth.service';
import { TaskAttachment, TaskItem } from '../../core/models/task.model';
import { TaskFormComponent } from '../task-form/task-form.component';

const MAX_FILE_SIZE = 5 * 1024 * 1024;

@Component({
  selector: 'app-task-list',
  standalone: true,
  imports: [CommonModule, TaskFormComponent],
  templateUrl: './task-list.component.html'
})
export class TaskListComponent implements OnInit {
  tasks: TaskItem[] = [];
  showForm = false;
  editingTask: TaskItem | null = null;

  // Tracks which task's attachment panel is expanded.
  expandedTaskId: number | null = null;

  constructor(private taskService: TaskService, public auth: AuthService) {}

  ngOnInit(): void {
    this.loadTasks();
  }

  loadTasks(): void {
    this.taskService.getAll().subscribe(data => this.tasks = data);
  }

  statusBadgeClass(task: TaskItem): string {
    if (task.isOverdue) return 'badge badge-overdue';
    if (task.status === 'Completed') return 'badge badge-completed';
    if (task.status === 'InProgress') return 'badge badge-inprogress';
    return 'badge badge-pending';
  }

  toggleAttachments(task: TaskItem): void {
    this.expandedTaskId = this.expandedTaskId === task.id ? null : task.id;
  }

  formatFileSize(bytes: number): string {
    if (bytes < 1024) return `${bytes} B`;
    if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
    return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
  }

  openAddForm(): void {
    this.editingTask = null;
    this.showForm = true;
  }

  openEditForm(task: TaskItem): void {
    if (task.status === 'Completed') return; // Completed tasks cannot be edited
    this.editingTask = task;
    this.showForm = true;
  }

  deleteTask(task: TaskItem): void {
    if (!confirm(`Delete task "${task.title}"?`)) return;
    this.taskService.delete(task.id).subscribe(() => this.loadTasks());
  }

  onFormClosed(saved: boolean): void {
    this.showForm = false;
    if (saved) this.loadTasks();
  }

  // ---- Attachments ----

  downloadAttachment(attachment: TaskAttachment): void {
    this.taskService.downloadAttachment(attachment.id).subscribe({
      next: (blob) => this.taskService.triggerDownload(blob, attachment.originalFileName),
      error: () => alert('You do not have permission to download this file, or it could not be found.')
    });
  }

  // Admin only: replace the main attachment (blocked server-side once task is completed).
  replaceMainAttachment(event: Event, task: TaskItem): void {
    const file = this.pickValidFile(event);
    if (!file) return;

    this.taskService.replaceMainAttachment(task.id, file).subscribe({
      next: () => this.loadTasks(),
      error: (err) => alert(err.error?.message || 'Failed to replace attachment.')
    });
  }

  // Employees (on their own task) or admins can add a supporting document.
  uploadSupportingDocument(event: Event, task: TaskItem): void {
    const file = this.pickValidFile(event);
    if (!file) return;

    this.taskService.uploadSupportingDocument(task.id, file).subscribe({
      next: () => this.loadTasks(),
      error: (err) => alert(err.error?.message || 'Upload failed.')
    });
  }

  // Admin only.
  deleteAttachment(attachment: TaskAttachment, task: TaskItem): void {
    if (!confirm(`Delete "${attachment.originalFileName}"?`)) return;

    this.taskService.deleteAttachment(attachment.id).subscribe({
      next: () => this.loadTasks(),
      error: () => alert('Failed to delete attachment.')
    });
  }

  private pickValidFile(event: Event): File | null {
    const input = event.target as HTMLInputElement;
    if (!input.files?.length) return null;

    const file = input.files[0];
    if (file.size > MAX_FILE_SIZE) {
      alert('File exceeds the 5 MB limit.');
      input.value = '';
      return null;
    }

    return file;
  }
}

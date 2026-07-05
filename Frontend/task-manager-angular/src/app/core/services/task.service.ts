import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { TaskAttachment, TaskCreateRequest, TaskItem, TaskUpdateRequest } from '../models/task.model';

import { API_BASE_URL } from '../api-config';
const API_URL = `${API_BASE_URL}/tasks`;

@Injectable({ providedIn: 'root' })
export class TaskService {
  constructor(private http: HttpClient) {}

  getAll(): Observable<TaskItem[]> {
    return this.http.get<TaskItem[]>(API_URL);
  }

  getById(id: number): Observable<TaskItem> {
    return this.http.get<TaskItem>(`${API_URL}/${id}`);
  }

  // Admin only. `file` is the optional initial attachment uploaded at creation time.
  create(task: TaskCreateRequest, file?: File | null): Observable<TaskItem> {
    const formData = this.toFormData(task);
    if (file) formData.append('file', file);
    return this.http.post<TaskItem>(API_URL, formData);
  }

  update(id: number, task: TaskUpdateRequest): Observable<void> {
    return this.http.put<void>(`${API_URL}/${id}`, task);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${API_URL}/${id}`);
  }

  // ---- Attachments ----

  getAttachments(taskId: number): Observable<TaskAttachment[]> {
    return this.http.get<TaskAttachment[]>(`${API_URL}/${taskId}/attachments`);
  }

  // Employees uploading supporting docs to their own task, or admins adding extra docs.
  uploadSupportingDocument(taskId: number, file: File): Observable<TaskAttachment> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<TaskAttachment>(`${API_URL}/${taskId}/attachments`, formData);
  }

  // Admin only — replaces the main attachment; blocked server-side once task is Completed.
  replaceMainAttachment(taskId: number, file: File): Observable<{ message: string }> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.put<{ message: string }>(`${API_URL}/${taskId}/attachment`, formData);
  }

  // Role-checked server-side: admin can download any; employee only their own task's files.
  downloadAttachment(attachmentId: number): Observable<Blob> {
    return this.http.get(`${API_URL}/attachments/${attachmentId}/download`, { responseType: 'blob' });
  }

  // Admin only.
  deleteAttachment(attachmentId: number): Observable<void> {
    return this.http.delete<void>(`${API_URL}/attachments/${attachmentId}`);
  }

  triggerDownload(blob: Blob, fileName: string): void {
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = fileName;
    a.click();
    window.URL.revokeObjectURL(url);
  }

  private toFormData(task: TaskCreateRequest): FormData {
    const formData = new FormData();
    formData.append('Title', task.title);
    formData.append('Description', task.description ?? '');
    formData.append('Priority', task.priority);
    formData.append('StartDate', task.startDate);
    formData.append('DueDate', task.dueDate);
    formData.append('AssignedEmployeeId', String(task.assignedEmployeeId));
    return formData;
  }
}

export type TaskPriority = 'Low' | 'Medium' | 'High';
export type TaskStatus = 'Pending' | 'InProgress' | 'Completed';

export interface TaskAttachment {
  id: number;
  originalFileName: string;
  contentType: string;
  fileSizeBytes: number;
  isMain: boolean;
  uploadedAt: string;
  uploadedByRole: 'Admin' | 'Employee';
}

export interface TaskItem {
  id: number;
  title: string;
  description: string;
  priority: TaskPriority;
  status: TaskStatus;
  startDate: string;
  dueDate: string;
  assignedEmployeeId: number;
  assignedEmployeeName: string;
  isOverdue: boolean;
  attachments: TaskAttachment[];
}

export interface TaskCreateRequest {
  title: string;
  description: string;
  priority: TaskPriority;
  startDate: string;
  dueDate: string;
  assignedEmployeeId: number;
}

export interface TaskUpdateRequest extends TaskCreateRequest {
  status: TaskStatus;
}

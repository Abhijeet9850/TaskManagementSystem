export interface AppNotification {
  id: number;
  message: string;
  type: 'TaskAssigned' | 'TaskDueSoon' | 'TaskCompleted';
  isRead: boolean;
  createdAt: string;
  taskId?: number;
}

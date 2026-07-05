export interface Employee {
  id: number;
  name: string;
  email: string;
  department: string;
  designation: string;
  totalTasks?: number;
  completedTasks?: number;
  linkedUserEmail?: string | null;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

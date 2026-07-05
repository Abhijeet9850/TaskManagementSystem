import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { API_BASE_URL } from '../api-config';
const API_URL = `${API_BASE_URL}/reports`;

export type ReportType = 'Completed' | 'Pending' | 'EmployeeWise';
export type ExportFormat = 'Excel' | 'Csv';

@Injectable({ providedIn: 'root' })
export class ReportService {
  constructor(private http: HttpClient) {}

  download(type: ReportType, format: ExportFormat): Observable<Blob> {
    return this.http.get(API_URL, {
      params: { type, format },
      responseType: 'blob'
    });
  }

  triggerDownload(blob: Blob, type: ReportType, format: ExportFormat): void {
    const extension = format === 'Excel' ? 'xlsx' : 'csv';
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `${type}Report.${extension}`;
    a.click();
    window.URL.revokeObjectURL(url);
  }
}

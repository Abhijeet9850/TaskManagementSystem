import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ExportFormat, ReportService, ReportType } from '../../core/services/report.service';

@Component({
  selector: 'app-reports',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './reports.component.html'
})
export class ReportsComponent {
  reportTypes: { label: string; value: ReportType }[] = [
    { label: 'Completed Tasks', value: 'Completed' },
    { label: 'Pending Tasks', value: 'Pending' },
    { label: 'Employee wise Tasks', value: 'EmployeeWise' }
  ];

  loadingKey = '';

  constructor(private reportService: ReportService) {}

  download(type: ReportType, format: ExportFormat): void {
    this.loadingKey = `${type}-${format}`;
    this.reportService.download(type, format).subscribe({
      next: (blob) => {
        this.reportService.triggerDownload(blob, type, format);
        this.loadingKey = '';
      },
      error: () => this.loadingKey = ''
    });
  }
}

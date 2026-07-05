using ClosedXML.Excel;
using CsvHelper;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;
using TaskManager.API.Data;
using TaskManager.API.Models;

namespace TaskManager.API.Services
{
    public class ReportService : IReportService
    {
        private readonly AppDbContext _db;

        public ReportService(AppDbContext db)
        {
            _db = db;
        }

        private class ReportRow
        {
            public string TaskTitle { get; set; } = string.Empty;
            public string EmployeeName { get; set; } = string.Empty;
            public string Department { get; set; } = string.Empty;
            public string Priority { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty;
            public string StartDate { get; set; } = string.Empty;
            public string DueDate { get; set; } = string.Empty;
        }

        public async Task<(byte[] content, string contentType, string fileName)> GenerateReportAsync(ReportType type, ExportFormat format)
        {
            var query = _db.Tasks.Include(t => t.AssignedEmployee).AsQueryable();

            query = type switch
            {
                ReportType.Completed => query.Where(t => t.Status == TaskItemStatus.Completed),
                ReportType.Pending => query.Where(t => t.Status != TaskItemStatus.Completed),
                ReportType.EmployeeWise => query.OrderBy(t => t.AssignedEmployee!.Name),
                _ => query
            };

            var tasks = await query.ToListAsync();

            var rows = tasks.Select(t => new ReportRow
            {
                TaskTitle = t.Title,
                EmployeeName = t.AssignedEmployee?.Name ?? "Unassigned",
                Department = t.AssignedEmployee?.Department ?? "-",
                Priority = t.Priority.ToString(),
                Status = t.Status.ToString(),
                StartDate = t.StartDate.ToString("yyyy-MM-dd"),
                DueDate = t.DueDate.ToString("yyyy-MM-dd")
            }).ToList();

            var baseFileName = type switch
            {
                ReportType.Completed => "CompletedTasksReport",
                ReportType.Pending => "PendingTasksReport",
                ReportType.EmployeeWise => "EmployeeWiseTasksReport",
                _ => "Report"
            };

            if (format == ExportFormat.Excel)
            {
                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Report");

                worksheet.Cell(1, 1).Value = "Task Title";
                worksheet.Cell(1, 2).Value = "Employee";
                worksheet.Cell(1, 3).Value = "Department";
                worksheet.Cell(1, 4).Value = "Priority";
                worksheet.Cell(1, 5).Value = "Status";
                worksheet.Cell(1, 6).Value = "Start Date";
                worksheet.Cell(1, 7).Value = "Due Date";
                worksheet.Row(1).Style.Font.Bold = true;

                for (int i = 0; i < rows.Count; i++)
                {
                    var r = rows[i];
                    var rowIndex = i + 2;
                    worksheet.Cell(rowIndex, 1).Value = r.TaskTitle;
                    worksheet.Cell(rowIndex, 2).Value = r.EmployeeName;
                    worksheet.Cell(rowIndex, 3).Value = r.Department;
                    worksheet.Cell(rowIndex, 4).Value = r.Priority;
                    worksheet.Cell(rowIndex, 5).Value = r.Status;
                    worksheet.Cell(rowIndex, 6).Value = r.StartDate;
                    worksheet.Cell(rowIndex, 7).Value = r.DueDate;
                }

                worksheet.Columns().AdjustToContents();

                using var ms = new MemoryStream();
                workbook.SaveAs(ms);
                return (ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{baseFileName}.xlsx");
            }
            else
            {
                using var ms = new MemoryStream();
                using var writer = new StreamWriter(ms, Encoding.UTF8, leaveOpen: true);
                using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
                csv.WriteRecords(rows);
                writer.Flush();
                return (ms.ToArray(), "text/csv", $"{baseFileName}.csv");
            }
        }
    }

}

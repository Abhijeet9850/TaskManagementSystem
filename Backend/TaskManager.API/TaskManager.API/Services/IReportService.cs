namespace TaskManager.API.Services
{
    public enum ReportType
    {
        Completed,
        Pending,
        EmployeeWise
    }

    public enum ExportFormat
    {
        Excel,
        Csv
    }

    public interface IReportService
    {
        Task<(byte[] content, string contentType, string fileName)> GenerateReportAsync(ReportType type, ExportFormat format);
    }

}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.API.Services;

namespace TaskManager.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportsController(IReportService reportService)
        {
            _reportService = reportService;
        }

        // GET api/reports?type=Completed|Pending|EmployeeWise&format=Excel|Csv
        [HttpGet]
        public async Task<IActionResult> GetReport([FromQuery] ReportType type, [FromQuery] ExportFormat format = ExportFormat.Excel)
        {
            var (content, contentType, fileName) = await _reportService.GenerateReportAsync(type, format);
            return File(content, contentType, fileName);
        }
    }
}

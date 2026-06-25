using System.Threading.Tasks;
using Ecommerce.Contracts.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminDashboardController : ControllerBase
    {
        private readonly IAdminDashboardService _adminDashboardService;

        public AdminDashboardController(IAdminDashboardService adminDashboardService)
        {
            _adminDashboardService = adminDashboardService;
        }

        [HttpGet("kpis")]
        public async Task<IActionResult> GetKpis([FromQuery] string? month)
        {
            var response = await _adminDashboardService.GetKpis(month);
            return Ok(response);
        }

        [HttpGet("revenue-breakdown")]
        public async Task<IActionResult> GetRevenueBreakdown()
        {
            var response = await _adminDashboardService.GetRevenueBreakdown();
            return Ok(response);
        }

        [HttpGet("order-status")]
        public async Task<IActionResult> GetOrderStatus()
        {
            var response = await _adminDashboardService.GetOrderStatus();
            return Ok(response);
        }

        [HttpGet("performance")]
        public async Task<IActionResult> GetPerformanceMetrics([FromQuery] string? month)
        {
            var response = await _adminDashboardService.GetPerformanceMetrics(month);
            return Ok(response);
        }

        [HttpGet("activity")]
        public async Task<IActionResult> GetRecentActivity()
        {
            var response = await _adminDashboardService.GetRecentActivity();
            return Ok(response);
        }

        [HttpGet("discount-distribution")]
        public async Task<IActionResult> GetDiscountDistribution()
        {
            var response = await _adminDashboardService.GetDiscountDistribution();
            return Ok(response);
        }

        [HttpGet("export")]
        public async Task<IActionResult> ExportReport([FromQuery] string? month)
        {
            var (fileBytes, fileName) = await _adminDashboardService.ExportReport(month);
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        [HttpPost("notifications/mark-all-read")]
        public async Task<IActionResult> MarkAllNotificationsRead()
        {
            await _adminDashboardService.MarkAllNotificationsRead();
            return Ok();
        }
    }
}

using System.Threading.Tasks;
using Ecommerce.Models.DTOs;

namespace Ecommerce.Contracts.Services
{
    public interface IAdminDashboardService
    {
        Task<DashboardStatsDTO> GetKpis(string? month);
        Task<RevenueBreakdownDTO> GetRevenueBreakdown();
        Task<OrderStatusDTO> GetOrderStatus();
        Task<PerformanceMetricsDTO> GetPerformanceMetrics(string? month);
        Task<RecentActivityDTO> GetRecentActivity();
        Task<DiscountDistributionDTO> GetDiscountDistribution();
        Task<(byte[] FileBytes, string FileName)> ExportReport(string? month);
        Task MarkAllNotificationsRead();
    }
}
namespace Ecommerce.Models.DTOs
{
    public class DashboardDataResponse
    {
        public DashboardStatsDTO Stats { get; set; } = null!;
        public RevenueBreakdownDTO RevenueBreakdown { get; set; } = null!;
        public OrderStatusDTO OrderStatus { get; set; } = null!;
        public ICollection<RecentOrderDTO> RecentOrders { get; set; } = [];
        public ICollection<TopSellingProductDTO> TopSellingProducts { get; set; } = [];
        public DiscountDistributionDTO DiscountDistribution { get; set; } = null!;
        public ICollection<RecentNotificationDTO> RecentNotifications { get; set; } = [];
        public ICollection<CategoryPerformanceDTO> CategoryPerformance { get; set; } = [];
        public ICollection<VendorPerformanceDTO> VendorPerformance { get; set; } = [];
    }
}
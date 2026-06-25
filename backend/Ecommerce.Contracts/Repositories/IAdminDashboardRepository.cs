using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ecommerce.Models;

namespace Ecommerce.Contracts.Repositories
{
    public interface IAdminDashboardRepository
    {
        Task<decimal> GetRevenueFromCustomer(DateTime? startDate, DateTime? endDate);
        Task<decimal> GetRevenueFromVendor(DateTime? startDate, DateTime? endDate);
        Task<decimal> GetRevenue(DateTime? startDate, DateTime? endDate);
        Task<int> GetOrdersCount(DateTime startDate, DateTime endDate);
        Task<int> GetProductsCount(DateTime endDate);
        Task<int> GetVendorsCount(DateTime endDate);
        Task<List<(OrderStatus Status, int Count)>> GetOrderStatusCounts();
        Task<List<Order>> GetRecentOrders(int count);
        Task<List<(string Name, string Category, int UnitsSold, decimal Revenue)>> GetTopSellingProducts(DateTime startDate, DateTime endDate, int count);
        Task<List<(string Code, decimal Amount)>> GetDiscountDistribution(int count);
        Task<List<Notification>> GetRecentNotifications(int count);
        Task MarkAllNotificationsAsRead();
        Task<List<(string CategoryName, int OrdersCount)>> GetCategoryPerformance(DateTime startDate, DateTime endDate);
        Task<List<(string StoreName, decimal Revenue)>> GetVendorPerformance(DateTime startDate, DateTime endDate, int count);
    }
}
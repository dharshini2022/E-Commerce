using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ecommerce.Contracts.Repositories;
using Ecommerce.DAL.Context;
using Ecommerce.Models;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.DAL.Repositories
{
    public class AdminDashboardRepository : IAdminDashboardRepository
    {
        private readonly AppDbContext _dbContext;

        public AdminDashboardRepository(AppDbContext context)
        {
            _dbContext = context;
        }

        public async Task<decimal> GetRevenueFromCustomer(DateTime? startDate, DateTime? endDate)
        {
            var query = _dbContext.Orders
                .Where(o => o.OrderPaymentStatus == PaymentStatus.Paid);

            if (startDate.HasValue) query = query.Where(o => o.PlacedAt >= startDate.Value);

            if (endDate.HasValue) query = query.Where(o => o.PlacedAt <= endDate.Value);

            return await query.SumAsync(o => (decimal?)o.PlatformCommission) ?? 0;
        }

        public async Task<decimal> GetRevenueFromVendor(DateTime? startDate, DateTime? endDate)
        {
            var query = _dbContext.Set<VendorSettlement>().AsQueryable();

            if (startDate.HasValue) query = query.Where(v => v.SettledAt >= startDate.Value);

            if (endDate.HasValue)  query = query.Where(v => v.SettledAt <= endDate.Value);

            return await query.SumAsync(v => (decimal?)v.PlatformCommissionAmount) ?? 0;
        }

        public async Task<decimal> GetRevenue(DateTime? startDate, DateTime? endDate)
        {
            var customerRevenue = await GetRevenueFromCustomer(startDate, endDate);
            var vendorRevenue = await GetRevenueFromVendor(startDate, endDate);

            return customerRevenue + vendorRevenue;
        }       

        public async Task<int> GetOrdersCount(DateTime startDate, DateTime endDate)
        {
            return await _dbContext.Orders
                .Where(o => o.PlacedAt >= startDate && o.PlacedAt <= endDate && o.Status == OrderStatus.Confirmed)
                .CountAsync();
        }

        public async Task<int> GetProductsCount(DateTime endDate)
        {
            return await _dbContext.Products
                .Where(p => p.Status == ProductStatus.Active && p.CreatedAt <= endDate)
                .CountAsync();
        }

        public async Task<int> GetVendorsCount(DateTime endDate)
        {
            return await _dbContext.Vendors
                .Where(v => v.IsActive && v.Status == VendorStatus.Approved && v.ApprovedAt != null && v.ApprovedAt <= endDate)
                .CountAsync();
        }

        public async Task<List<(OrderStatus Status, int Count)>> GetOrderStatusCounts()
        {
            var statusCounts = await _dbContext.Orders
                .GroupBy(o => o.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            return statusCounts.Select(x => (x.Status, x.Count)).ToList();
        }

        public async Task<List<Order>> GetRecentOrders(int count)
        {
            return await _dbContext.Orders
                .Include(o => o.User)
                .OrderByDescending(o => o.PlacedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<(string Name, string Category, int UnitsSold, decimal Revenue)>> GetTopSellingProducts(DateTime startDate, DateTime endDate, int count)
        {
            var topProductsQuery = await _dbContext.OrderItems
                .Where(oi => oi.Order.PlacedAt >= startDate && oi.Order.PlacedAt <= endDate && oi.Order.OrderPaymentStatus == PaymentStatus.Paid)
                .Include(oi => oi.Variant)
                .ThenInclude(v => v.Product)
                .ThenInclude(p => p.Category)
                .GroupBy(oi => new { oi.Variant.Product.Id, oi.Variant.Product.Name, CategoryName = oi.Variant.Product.Category.Name })
                .Select(g => new {
                    Name = g.Key.Name,
                    Category = g.Key.CategoryName,
                    UnitsSold = g.Sum(oi => oi.Quantity),
                    Revenue = g.Sum(oi => oi.UnitPrice * oi.Quantity)
                })
                .OrderByDescending(x => x.UnitsSold)
                .Take(count)
                .ToListAsync();

            return topProductsQuery.Select(x => (x.Name, x.Category, x.UnitsSold, x.Revenue)).ToList();
        }

        public async Task<List<(string Code, decimal Amount)>> GetDiscountDistribution(int count)
        {
            var discountsQuery = await _dbContext.Orders
                .Where(o => o.DiscountId != null && o.OrderPaymentStatus == PaymentStatus.Paid)
                .Include(o => o.Discount)
                .GroupBy(o => o.Discount!.Code)
                .Select(g => new {
                    Code = g.Key,
                    Amount = g.Sum(o => o.DiscountAmount)
                })
                .OrderByDescending(x => x.Amount)
                .Take(count)
                .ToListAsync();

            return discountsQuery.Select(x => (x.Code, x.Amount)).ToList();
        }

        public async Task<List<Notification>> GetRecentNotifications(int count)
        {
            return await _dbContext.Notifications
                .OrderByDescending(n => n.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task MarkAllNotificationsAsRead()
        {
            var unreadNotifications = await _dbContext.Notifications
                .Where(n => !n.IsRead)
                .ToListAsync();

            foreach (var note in unreadNotifications)
            {
                note.IsRead = true;
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<(string CategoryName, int OrdersCount)>> GetCategoryPerformance(DateTime startDate, DateTime endDate)
        {
            var categoryPerformanceQuery = await _dbContext.OrderItems
                .Where(oi => oi.Order.PlacedAt >= startDate && oi.Order.PlacedAt <= endDate)
                .Include(oi => oi.Variant)
                .ThenInclude(v => v.Product)
                .ThenInclude(p => p.Category)
                .GroupBy(oi => oi.Variant.Product.Category.Name)
                .Select(g => new {
                    Category = g.Key,
                    Count = g.Select(oi => oi.OrderId).Distinct().Count()
                })
                .OrderByDescending(x => x.Count)
                .ToListAsync();

            return categoryPerformanceQuery.Select(x => (x.Category, x.Count)).ToList();
        }

        public async Task<List<(string StoreName, decimal Revenue)>> GetVendorPerformance(DateTime startDate, DateTime endDate, int count)
        {
            var vendorPerformanceQuery = await _dbContext.OrderItems
                .Where(oi => oi.Order.PlacedAt >= startDate && oi.Order.PlacedAt <= endDate && oi.Order.OrderPaymentStatus == PaymentStatus.Paid)
                .Include(oi => oi.Vendor)
                .GroupBy(oi => oi.Vendor.StoreName)
                .Select(g => new {
                    StoreName = g.Key,
                    Revenue = g.Sum(oi => oi.Quantity * oi.UnitPrice)
                })
                .OrderByDescending(x => x.Revenue)
                .Take(count)
                .ToListAsync();

            return vendorPerformanceQuery.Select(x => (x.StoreName, x.Revenue)).ToList();
        }
    }
}
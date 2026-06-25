using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Ecommerce.Contracts.Repositories;
using Ecommerce.Contracts.Services;
using Ecommerce.Models;
using Ecommerce.Models.DTOs;

namespace Ecommerce.BLL
{
    public class DashboardDateRange
    {
        public DateTime CurrentStart { get; set; }
        public DateTime CurrentEnd { get; set; }
        public DateTime PreviousStart { get; set; }
        public DateTime PreviousEnd { get; set; }
    }
    public class AdminDashboardService : IAdminDashboardService
    {
        private readonly IAdminDashboardRepository _adminDashboardRepository;
        private readonly IMapper _mapper;
        private DashboardDateRange _dates;

        public AdminDashboardService(IAdminDashboardRepository adminDashboardRepository, IMapper mapper)
        {
            _adminDashboardRepository = adminDashboardRepository;
            _mapper = mapper;
            _dates = new DashboardDateRange();
        }

        public async Task<DashboardStatsDTO> GetKpis(string? month)
        {
            var range = GetDateRange(month);

            var currentRevenue =await _adminDashboardRepository.GetRevenue(range.CurrentStart,range.CurrentEnd);
            var previousRevenue =await _adminDashboardRepository.GetRevenue(range.PreviousStart,range.PreviousEnd);

            var currentOrders =await _adminDashboardRepository.GetOrdersCount(range.CurrentStart,range.CurrentEnd);
            var previousOrders =await _adminDashboardRepository.GetOrdersCount(range.PreviousStart,range.PreviousEnd);

            var currentProducts =await _adminDashboardRepository.GetProductsCount(range.CurrentEnd);
            var previousProducts =await _adminDashboardRepository.GetProductsCount(range.PreviousEnd);

            var currentVendors =await _adminDashboardRepository.GetVendorsCount(range.CurrentEnd);
            var previousVendors =await _adminDashboardRepository.GetVendorsCount(range.PreviousEnd);

            return new DashboardStatsDTO
            {
                TotalRevenue = CreateStatCard(currentRevenue, previousRevenue),
                TotalOrders = CreateStatCard(currentOrders, previousOrders),
                ActiveProducts = CreateStatCard(currentProducts, previousProducts),
                ActiveVendors = CreateStatCard(currentVendors, previousVendors)
            };
        }

        private DashboardDateRange GetDateRange(string? month)
        {
            DateTime selectedMonthStart;

            if (string.IsNullOrEmpty(month) || !DateTime.TryParseExact(month,"yyyy-MM",CultureInfo.InvariantCulture,DateTimeStyles.None,out selectedMonthStart))
            {
                selectedMonthStart = new DateTime(DateTime.Now.Year,DateTime.Now.Month,1);
            }

            _dates.CurrentStart = selectedMonthStart;
            _dates.CurrentEnd = selectedMonthStart.AddMonths(1).AddTicks(-1);
            _dates.PreviousStart = selectedMonthStart.AddMonths(-1);
            _dates.PreviousEnd = selectedMonthStart.AddTicks(-1);
            return _dates;
        }

        private StatCardDTO CreateStatCard(decimal current, decimal previous)
        {
            var change = CalculateChangePercent(current, previous);
            return new StatCardDTO
            {
                Value = current,
                ChangePercent = Math.Abs(change),
                ChangeDirection = change >= 0 ? "up" : "down"
            };
        }

        public async Task<RevenueBreakdownDTO> GetRevenueBreakdown()
        {
            var targetYear = DateTime.Now.Year;
            var monthlyPoints = new List<RevenuePointDTO>();
            for (int m = 1; m <= 12; m++)
            {
                var monthStart = new DateTime(targetYear, m, 1);
                var monthEnd = monthStart.AddMonths(1).AddTicks(-1);
                var monthRevenue = await _adminDashboardRepository.GetRevenue(monthStart, monthEnd);

                monthlyPoints.Add(new RevenuePointDTO
                {
                    Label = monthStart.ToString("MMM", CultureInfo.InvariantCulture),
                    Revenue = monthRevenue
                });
            }

            return new RevenueBreakdownDTO
            {
                Monthly = monthlyPoints
            };
        }

        public async Task<OrderStatusDTO> GetOrderStatus()
        {
            var statusCounts = await _adminDashboardRepository.GetOrderStatusCounts();
            int totalOrders = statusCounts.Sum(x => x.Count);
            var orderStatusSlices = new List<DonutSliceDTO>();
            var statusColors = new Dictionary<OrderStatus, string>
            {
                { OrderStatus.Delivered, "#10B981" },
                { OrderStatus.Shipped, "#3B82F6" },
                { OrderStatus.Confirmed, "#F59E0B" },
                { OrderStatus.Cancelled, "#EF4444" },
                { OrderStatus.PendingPayment, "#8B5CF6" },
                { OrderStatus.PaymentFailed, "#EC4899" }
            };

            foreach (var sc in statusCounts)
            {
                var color = statusColors[sc.Status];
                orderStatusSlices.Add(new DonutSliceDTO
                {
                    Label = sc.Status.ToString(),
                    Color = color,
                    Percentage = totalOrders == 0 ? 0 : Math.Round((decimal)sc.Count * 100 / totalOrders, 1)
                });
            }

            if (!orderStatusSlices.Any())
            {
                orderStatusSlices.Add(new DonutSliceDTO
                {
                    Label = "No Orders",
                    Color = "#E5E7EB",
                    Percentage = 100
                });
            }

            return new OrderStatusDTO
            {
                TotalOrders = totalOrders,
                Slices = orderStatusSlices
            };
        }

        public async Task<PerformanceMetricsDTO> GetPerformanceMetrics(string? month)
        {
            DateTime selectedMonthStart;
            if (string.IsNullOrEmpty(month) || !DateTime.TryParseExact(month, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out selectedMonthStart))
            {
                selectedMonthStart = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            }
            var selectedMonthEnd = selectedMonthStart.AddMonths(1).AddTicks(-1);

            var dbTopSelling = await _adminDashboardRepository.GetTopSellingProducts(selectedMonthStart, selectedMonthEnd, 5);
            var topSellingProducts = dbTopSelling.Select((p, idx) => new TopSellingProductDTO
            {
                Rank = idx + 1,
                Name = p.Name,
                Category = p.Category,
                UnitsSold = p.UnitsSold,
                Revenue = p.Revenue
            }).ToList();

            var dbCategoryPerf = await _adminDashboardRepository.GetCategoryPerformance(selectedMonthStart, selectedMonthEnd);
            int totalCategoryOrders = dbCategoryPerf.Sum(x => x.OrdersCount);
            var categoryPerformance = dbCategoryPerf.Select(c => new CategoryPerformanceDTO
            {
                Category = c.CategoryName,
                Orders = c.OrdersCount,
                Percentage = totalCategoryOrders == 0 ? 0 : Math.Round((decimal)c.OrdersCount * 100 / totalCategoryOrders, 1)
            }).ToList();

            var dbVendorPerf = await _adminDashboardRepository.GetVendorPerformance(selectedMonthStart, selectedMonthEnd, 5);
            decimal maxVendorRevenue = dbVendorPerf.Any() ? dbVendorPerf.Max(x => x.Revenue) : 1;
            var vendorPerformance = dbVendorPerf.Select((v, idx) => new VendorPerformanceDTO
            {
                Rank = idx + 1,
                Name = v.StoreName,
                Revenue = v.Revenue,
                Percentage = Math.Round(v.Revenue * 100 / maxVendorRevenue, 1)
            }).ToList();

            return new PerformanceMetricsDTO
            {
                TopSellingProducts = topSellingProducts,
                CategoryPerformance = categoryPerformance,
                VendorPerformance = vendorPerformance
            };
        }

        public async Task<RecentActivityDTO> GetRecentActivity()
        {
            var dbRecentOrders = await _adminDashboardRepository.GetRecentOrders(5);
            var recentOrders = _mapper.Map<List<RecentOrderDTO>>(dbRecentOrders);

            var dbNotifications = await _adminDashboardRepository.GetRecentNotifications(5);
            var recentNotifications = dbNotifications.Select(n => new RecentNotificationDTO
            {
                Id = n.Id,
                Message = n.Message,
                Type = n.Level.ToString().ToLower(),
                NotifiedAt = n.CreatedAt,
                TimeAgo = GetTimeAgo(n.CreatedAt),
                Read = n.IsRead
            }).ToList();

            return new RecentActivityDTO
            {
                RecentOrders = recentOrders,
                RecentNotifications = recentNotifications
            };
        }

        public async Task<DiscountDistributionDTO> GetDiscountDistribution()
        {
            var dbDiscounts = await _adminDashboardRepository.GetDiscountDistribution(5);
            var totalDiscountAmount = dbDiscounts.Sum(x => x.Amount);
            var discountSlices = new List<DonutSliceDTO>();
            var discountColors = new[] { "#EC4899", "#8B5CF6", "#F59E0B", "#10B981", "#3B82F6" };
            for (int i = 0; i < dbDiscounts.Count; i++)
            {
                var dq = dbDiscounts[i];
                var pct = totalDiscountAmount == 0 ? 0 : Math.Round(dq.Amount * 100 / totalDiscountAmount, 1);
                discountSlices.Add(new DonutSliceDTO
                {
                    Label = dq.Code,
                    Percentage = pct,
                    Color = discountColors[i % discountColors.Length]
                });
            }

            if (!discountSlices.Any())
            {
                discountSlices.Add(new DonutSliceDTO
                {
                    Label = "No Promotions Used",
                    Percentage = 100,
                    Color = "#E5E7EB"
                });
            }

            return new DiscountDistributionDTO
            {
                TotalTypes = dbDiscounts.Count,
                Slices = discountSlices
            };
        }

        public async Task<(byte[] FileBytes, string FileName)> ExportReport(string? month)
        {
            DateTime selectedMonthStart;
            if (string.IsNullOrEmpty(month) || !DateTime.TryParseExact(month, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out selectedMonthStart))
            {
                selectedMonthStart = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            }

            var selectedMonthEnd = selectedMonthStart.AddMonths(1).AddTicks(-1);

            var totalRevenue = await _adminDashboardRepository.GetRevenue(selectedMonthStart, selectedMonthEnd);
            var totalOrders = await _adminDashboardRepository.GetOrdersCount(selectedMonthStart, selectedMonthEnd);
            var activeProducts = await _adminDashboardRepository.GetProductsCount(selectedMonthEnd);
            var activeVendors = await _adminDashboardRepository.GetVendorsCount(selectedMonthEnd);

            var csvBuilder = new StringBuilder();
            csvBuilder.AppendLine("E-Commerce Platform Performance Report");
            csvBuilder.AppendLine($"Report Month: {selectedMonthStart:yyyy-MM}");
            csvBuilder.AppendLine($"Generated At: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            csvBuilder.AppendLine();
            csvBuilder.AppendLine("Metric,Value");
            csvBuilder.AppendLine($"Total Revenue,INR {totalRevenue:F2}");
            csvBuilder.AppendLine($"Total Orders,{totalOrders}");
            csvBuilder.AppendLine($"Active Products,{activeProducts}");
            csvBuilder.AppendLine($"Active Vendors,{activeVendors}");

            var csvBytes = Encoding.UTF8.GetBytes(csvBuilder.ToString());
            string fileName = $"dashboard_report_{selectedMonthStart:yyyy_MM}.csv";
            return (csvBytes, fileName);
        }

        public async Task MarkAllNotificationsRead()
        {
            await _adminDashboardRepository.MarkAllNotificationsAsRead();
        }

        private static decimal CalculateChangePercent(decimal current, decimal previous)
        {
            if (previous == 0)
            {
                return current > 0 ? 100 : 0;
            }
            return Math.Round(((current - previous) / previous) * 100, 2);
        }

        private static string GetTimeAgo(DateTime date)
        {
            var span = DateTime.Now - date;
            if (span.TotalDays > 365)
            {
                int years = (int)(span.TotalDays / 365);
                return years == 1 ? "1 year ago" : $"{years} years ago";
            }
            if (span.TotalDays > 30)
            {
                int months = (int)(span.TotalDays / 30);
                return months == 1 ? "1 month ago" : $"{months} months ago";
            }
            if (span.TotalDays > 7)
            {
                int weeks = (int)(span.TotalDays / 7);
                return weeks == 1 ? "1 week ago" : $"{weeks} weeks ago";
            }
            if (span.TotalDays >= 1)
            {
                int days = (int)span.TotalDays;
                return days == 1 ? "yesterday" : $"{days} days ago";
            }
            if (span.TotalHours >= 1)
            {
                int hours = (int)span.TotalHours;
                return hours == 1 ? "1 hour ago" : $"{hours} hours ago";
            }
            if (span.TotalMinutes >= 1)
            {
                int minutes = (int)span.TotalMinutes;
                return minutes == 1 ? "1 min ago" : $"{minutes} mins ago";
            }
            return "just now";
        }
    }
}

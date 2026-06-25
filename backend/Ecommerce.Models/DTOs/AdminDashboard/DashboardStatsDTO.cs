using System;
using System.Collections.Generic;

namespace Ecommerce.Models.DTOs
{
    public class DashboardStatsDTO
    {
        public StatCardDTO TotalRevenue { get; set; } = null!;
        public StatCardDTO TotalOrders { get; set; } = null!;
        public StatCardDTO ActiveProducts { get; set; } = null!;
        public StatCardDTO ActiveVendors { get; set; } = null!;
    }
}
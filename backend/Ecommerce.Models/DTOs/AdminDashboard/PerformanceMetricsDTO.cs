using System.Collections.Generic;

namespace Ecommerce.Models.DTOs
{
    public class PerformanceMetricsDTO
    {
        public ICollection<TopSellingProductDTO> TopSellingProducts { get; set; } = [];
        public ICollection<CategoryPerformanceDTO> CategoryPerformance { get; set; } = [];
        public ICollection<VendorPerformanceDTO> VendorPerformance { get; set; } = [];
    }
}

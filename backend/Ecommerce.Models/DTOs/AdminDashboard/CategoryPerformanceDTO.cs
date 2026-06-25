namespace Ecommerce.Models.DTOs
{
    public class CategoryPerformanceDTO
    {
        public string Category { get; set; } = null!;
        public int Orders { get; set; }
        public decimal Percentage { get; set; }
    }
}
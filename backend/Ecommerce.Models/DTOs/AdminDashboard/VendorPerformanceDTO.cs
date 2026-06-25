namespace Ecommerce.Models.DTOs
{
    public class VendorPerformanceDTO
    {
        public int Rank { get; set; }
        public string Name { get; set; } = null!;
        public decimal Revenue { get; set; }
        public decimal Percentage { get; set; }
    }
}
namespace Ecommerce.Models.DTOs
{
    public class TopSellingProductDTO
    {
        public int Rank { get; set; }
        public string Name { get; set; } = null!;
        public string Category { get; set; } = null!;
        public int UnitsSold { get; set; }
        public decimal Revenue { get; set; }
    }
}
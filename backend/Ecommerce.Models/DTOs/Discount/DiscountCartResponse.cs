namespace Ecommerce.Models.DTOs
{
    public class DiscountCartResponse
    {
        public string Code { get; set; } = null!;
        public DiscountType Type { get; set; } = DiscountType.Percentage;        
        public decimal Value { get; set; }
    }
}
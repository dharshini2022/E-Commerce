namespace Ecommerce.Models.DTOs
{
    public class OrderResponse
    {
        public int OrderId { get; set; }
        public decimal Subtotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal ShippingAmount { get; set; }
        public decimal PlatformCommission { get; set; }
        public decimal Total { get; set; }
        public string Currency { get; set; } = "inr";
        public string Message { get; set; } = string.Empty;
    }
}
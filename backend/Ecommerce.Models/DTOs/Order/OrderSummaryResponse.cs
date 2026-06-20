namespace Ecommerce.Models.DTOs
{
    public class OrderSummaryResponse
    {
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public string FullName { get; set; } = null!;
        public decimal Subtotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal ShippingAmount { get; set; }
        public decimal PlatformCommission { get; set; }
        public decimal Total { get; set; }
        public OrderStatus Status { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public DateTime PlacedAt { get; set; }
        public ICollection<OrderItemDTO> Items { get; set; } = new List<OrderItemDTO>();
    }
}
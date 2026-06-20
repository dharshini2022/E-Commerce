namespace Ecommerce.Models
{

    public class StockReservation
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int VariantId { get; set; }
        public int Quantity { get; set; }
        public DateTime ReservedAt { get; set; } = DateTime.Now;
        public bool IsReleased { get; set; } = false;
        public DateTime? ReleasedAt { get; set; }

        // Navigation
        public Order Order { get; set; } = null!;
        public ProductVariant Variant { get; set; } = null!;
    }
}

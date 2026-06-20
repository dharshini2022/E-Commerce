namespace Ecommerce.Models
{
    public enum ShipmentStatus
    {
        Pending = 1,
        Initiated = 2,
        Delivered = 3,
        Picked = 4,
        Cancelled = 5
    }
    public class Shipment
    {
        public int Id { get; set; }
        public string TrackingNumber { get; set; } = null!;
        public int UserAddressId { get; set; }
        public DateTime EstimatedFullfillement { get; set; }
        public DateTime? ShippedAt { get; set; }
        public DateTime? FulfilledAt { get; set; }
        public ShipmentStatus Status { get; set; }
        public decimal ShippingFee { get; set; }

        // Relation
        public ICollection<OrderItem> OrderItems { get; set; } = [];
        public Return? Return { get; set; } = null;
        public UserAddress UserAddress { get; set; } = null!;
    }
}
using Ecommerce.Models;

namespace Ecommerce.Models
{
    public enum OrderStatus
    {
        PendingPayment,      
        PaymentFailed,       
        Confirmed,           
        Shipped,
        Delivered,
        Cancelled,
        ReturnRequested,
        Returned,
        PartialReturnRequested,
        PartiallyReturned,
    }

    public enum OrderPaymentStatus
    {
        Pending,
        Paid,
        Failed,
        Refunded
    }
    public class Order
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int? DiscountId { get; set; }
        public int PaymentId { get; set; }
        public decimal Subtotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal ShippingAmount {get; set; }
        public decimal PlatformCommission { get; set; } = 20; 
        public decimal Total { get; set; }
        public OrderStatus Status { get; set; }
        public PaymentStatus OrderPaymentStatus { get; set; }
        public string? StripePaymentIntentId { get; set; } 
        public DateTime PlacedAt { get; set; } = DateTime.Now;
        public int? UserAddressId { get; set; }

        // Relation
        public User User { get; set; } = null!;
        public UserAddress? UserAddress { get; set; }
        public Discount? Discount { get; set; }
        public Payment Payment { get; set; } = null!;
        public ICollection<VendorSettlement> VendorSettlement {get; set; }= [];
        public ICollection<OrderItem> Items { get; set; } = [];
        public ICollection<Review> Reviews { get; set; } = [];
        public ICollection<Return> Returns {get; set;} = [];
    }

    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int VariantId { get; set; }
        public int VendorId { get; set; }
        public int? ShipmentId { get; set; }

        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }            

        // Navigation
        public Order Order { get; set; } = null!;
        public Shipment Shipment { get; set; } = null!;
        public Vendor Vendor { get; set; } = null!;

        public ProductVariant Variant { get; set; } = null!;
        public ICollection<ReturnItem> ReturnItems { get; set; } = [];
    
    }

}


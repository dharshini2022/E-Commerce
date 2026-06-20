namespace Ecommerce.Models
{
    public enum SettlementStatus
    {
        Pending,
        Paid,
        Failed
    }

    public class VendorSettlement
    {
        public int Id { get; set; }
        public int VendorId { get; set; }
        public int OrderId { get; set; }
        public decimal GrossAmount { get; set; }            
        public decimal ShippingAmount { get; set; }           
        public decimal VendorDiscountAmount { get; set; }      
        public decimal PlatformCommissionAmount { get; set; } 
        public decimal NetPayoutAmount { get; set; }           
        public string? TransactionReference { get; set; }     

        public SettlementStatus Status { get; set; }

        public DateTime? SettledAt { get; set; }

        // Navigation
        public Vendor Vendor { get; set; } = null!;

        public Order Order { get; set; } = null!;
    }
}

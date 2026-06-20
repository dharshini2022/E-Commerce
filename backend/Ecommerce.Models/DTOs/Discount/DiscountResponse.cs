namespace Ecommerce.Models.DTOs
{
    public class DiscountResponse
    {
        public int Id;
        public int? VendorId { get; set; }
        public int? ProductId {get; set; }
        public int? CategoryId { get; set; }
        public string Code { get; set; } = null!;
        public DiscountScope Scope { get; set; }         
        public DiscountType Type { get; set; }       
        public decimal Value { get; set; }
        public decimal MinOrderValue { get; set; }
        public int UsageLimit { get; set; }
        public int UsedCount { get; set; }
        public bool IsActive {get; set;}
        public DateTime ExpiresAt { get; set; }

    }
}
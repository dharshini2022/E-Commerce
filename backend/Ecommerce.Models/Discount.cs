namespace Ecommerce.Models
{
    public enum DiscountType
    {
        Percentage  = 1,
        Flat = 2
    }
    public enum DiscountScope
    {
        Common = 1,
        Vendor = 2,
        Category = 3,
        Product = 4
    }
    public class Discount
    {
        public int Id { get; set; }
        public int? VendorId { get; set; }
        public int? ProductId {get; set; }
        public int? CategoryId { get; set; }
        public string Code { get; set; } = null!;
        public DiscountScope Scope { get; set; } = DiscountScope.Common;          
        public DiscountType Type { get; set; } = DiscountType.Percentage;        
        public decimal Value { get; set; }
        public decimal MinOrderValue { get; set; }
        public int UsageLimit { get; set; }
        public int UsedCount { get; set; }
        public bool IsActive {get; set;}
        public DateTime ExpiresAt { get; set; }

        public Vendor? Vendor { get; set; } = null!;
        public Product? Product { get; set; } = null;
        public Category? Category { get; set; } = null;
        public ICollection<Order> Orders { get; set; } = [];
    }

}


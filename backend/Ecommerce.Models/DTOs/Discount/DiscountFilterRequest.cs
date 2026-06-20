namespace Ecommerce.Models.DTOs
{
    public class DiscountFilterRequest
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public DiscountType? discountType { get; set; }
        public int? CategoryId { get; set; }
        public int? ProductId { get; set; }
        public int? VendorId { get; set; }
        public int? MinValue { get; set; }
        public string? SortBy { get; set; } 
        public string? SortOrder { get; set; } 
        public string? SearchQuery { get; set; }
    }
}
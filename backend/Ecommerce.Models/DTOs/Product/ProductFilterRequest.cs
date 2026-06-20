namespace Ecommerce.Models.DTOs
{
    public class ProductFilterRequest
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int? CategoryId { get; set; }
        public string? SortBy { get; set; } 
        public string? SortOrder { get; set; } 
        public string? SearchQuery { get; set; }
    }
}
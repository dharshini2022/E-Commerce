namespace Ecommerce.Models.DTOs
{
    public class PageRequest
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 3;
        public string? SearchTerm { get; set; }
    }
}
namespace Ecommerce.Models.DTOs
{
    public class ProductResponse
    {
        public int Id { get; set; }
        public int VendorId { get; set; }
        public int CategoryId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public ProductStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public string StoreName { get; set; } = null!;
        public string CategoryName { get; set; } = null!;
        public List<ProductVariantResponse> Variants { get; set; } = new();
    }
    
}
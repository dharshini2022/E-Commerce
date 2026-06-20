namespace Ecommerce.Models.DTOs
{
    public class ProductVariantResponse
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int StockQty { get; set; }
        public decimal Price { get; set; }
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; }
        public Dictionary<string, string> AvailableValues { get; set; } = new();
        public ICollection<ProductImageResponse> VariantImages { get; set; } = new List<ProductImageResponse>();
    }
}
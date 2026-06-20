namespace Ecommerce.Models.DTOs
{
    public class ProductImageResponse
    {
        public int Id { get; set; }
        public int VariantId { get; set; }
        public string ImageUrl { get; set; } = null!;
        public int ImageOrder { get; set; }
    }
}
namespace Ecommerce.Models.DTOs
{
    public class WishListItemResponse
    {
        public int Id { get; set; }
        public int VariantId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public decimal UnitPrice { get; set; }
        public bool IsInStock { get; set; }
    }
}

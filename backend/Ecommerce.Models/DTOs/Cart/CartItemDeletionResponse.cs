namespace Ecommerce.Models.DTOs
{
    public class CartItemDeletionResponse
    {
        public string ProductName { get; set; } = null!;
        public decimal UnitPrice { get; set; }
    }
}
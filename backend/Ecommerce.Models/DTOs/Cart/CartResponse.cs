namespace Ecommerce.Models.DTOs
{
    public class CartResponse
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<CartItemResponse> Items { get; set; } = new();
        public decimal TotalAmount { get; set; }
        public int TotalItems { get; set; }
    }
}

using System.Collections.Concurrent;

namespace Ecommerce.Models
{
    public class Cart
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public User User { get; set; } = null!;
        public ICollection<CartItem> Items { get; set; } = [];
    }

    public class CartItem
    {
        public int Id { get; set; }
        public int CartId {get; set; }
        public int VariantId { get; set; }
        public int Quantity { get; set; }
        public Cart Cart { get; set; } = null!;
       public ProductVariant Variant { get; set; } = null!;
    }

}


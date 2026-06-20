namespace Ecommerce.Models;

public class Wishlist
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public bool IsPublic { get; set; }

    // Relation
    public User User { get; set; } = null!;
    public ICollection<WishlistItem> Items { get; set; } = [];
}

public class WishlistItem
{
    public int Id { get; set; }
    public int WishlistId { get; set; }
    public int VariantId { get; set; }
    public DateTime AddedAt { get; set; }

    // Relation
    public Wishlist Wishlist { get; set; } = null!;
    public ProductVariant Variant { get; set; } = null!;
}

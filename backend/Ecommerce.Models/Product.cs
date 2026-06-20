namespace Ecommerce.Models;
public enum ProductStatus
{
    Draft = 1,
    Active = 2,
    Archived = 3
}
public class Product
{
    public int Id { get; set; }
    public int VendorId { get; set; }
    public int CategoryId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public ProductStatus Status { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Relations
    public Vendor Vendor { get; set; } = null!;
    public Category Category { get; set; } = null!;
    public ICollection<ProductVariant> Variants { get; set; } = [];
    public ICollection<Review> Reviews { get; set; } = [];

    
}


public class ProductVariant
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int StockQty { get; set; }
    public int ReservedStockQty { get; set; }
    public decimal Price { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = false;
    public Dictionary<string, string> AvailableValues { get; set; } = new();

    //Relation
    public Product Product { get; set; } = null!;
    public ICollection<CartItem> CartItems { get; set; } = [];
    public ICollection<OrderItem> OrderItems { get; set; } = [];
    public ICollection<WishlistItem> WishlistItems { get; set; } = [];
    public ICollection<ProductImage> VariantImages { get; set; } = [];
}
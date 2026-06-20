using System.Collections.Concurrent;

namespace Ecommerce.Models;

public class DuplicateProductVariant
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string Label { get; set; } = string.Empty;         
    public string Value { get; set; } = string.Empty;      
    //+/- from base product price
    public decimal PriceDelta { get; set; }         
    public int StockQty { get; set; }
    public bool IsActive { get; set; } = true;

    // Relation
    public Product Product { get; set; } = null!;
    public ICollection<CartItem> CartItems { get; set; } = [];
    public ICollection<OrderItem> OrderItems { get; set; } = [];
    public ICollection<ProductImage> Images { get; set; } = [];
}

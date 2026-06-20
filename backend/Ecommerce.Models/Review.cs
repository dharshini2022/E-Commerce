namespace Ecommerce.Models;

public class Review
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int UserId { get; set; }
    public int OrderId { get; set; }              
    public decimal Rating { get; set; }              
    public string? Title { get; set; }
    public string? Body { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Relation
    public Product Product { get; set; } = null!;
    public User User { get; set; } = null!;
    public Order Order { get; set; } = null!;
    public ICollection<ReviewImage> ReviewImages { get; set; } = [];
}

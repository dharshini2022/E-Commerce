namespace Ecommerce.Models;
public enum UserRole
{
    Customer = 1,
    Vendor = 2,
    Admin = 3
}
public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public UserRole Role { get; set; } = UserRole.Customer;         
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Relation
    public Vendor? Vendor {get; set; }
    public Cart? Cart { get; set; }   
    public ICollection<UserAddress> Addresses { get; set; } = new List<UserAddress>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<Wishlist> Wishlists { get; set; } = new List<Wishlist>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public ICollection<RefreshToken> RefreshTokens   { get; set; } = new List<RefreshToken>();
}

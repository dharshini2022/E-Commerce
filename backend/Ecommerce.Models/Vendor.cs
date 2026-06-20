namespace Ecommerce.Models;
public enum VendorStatus
{
    Pending = 1,
    Approved = 2,
    Cancelled = 3
}

public class Vendor
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string StoreName { get; set; } = null!;
    public string? StoreEmail {get; set;} = null!;
    public string GSTNumber {get; set; } = null!;
    public string PANNumber {get; set; } = null!;
    public string? Description { get; set; }
    public VendorStatus Status { get; set; } = VendorStatus.Pending;
    public Boolean IsActive { get; set; } = false;
    public string? LogoUrl { get; set; }
    public DateTime? ApprovedAt { get; set; }


    // Navigation
    public User User { get; set; } = null!;
    public  ICollection<VendorSettlement> VendorSettlement {get; set;} = [];
    public ICollection<Product> Products { get; set; } = [];
    public ICollection<Discount> Discounts { get; set; } = [];
    public ICollection<OrderItem> OrderItems { get; set; } = [];
}

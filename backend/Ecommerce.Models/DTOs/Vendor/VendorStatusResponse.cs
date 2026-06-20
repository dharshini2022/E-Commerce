namespace Ecommerce.Models.DTOs
{
    public class VendorStatusResponse
    {
        public int Id { get; set; }
        public string StoreName { get; set; } = null!;
        public string Status { get; set; } = null!;
        public bool IsActive { get; set; }
    }
}
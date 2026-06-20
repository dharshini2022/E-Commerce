namespace Ecommerce.Models.DTOs
{
    public class VendorProfileResponse
    {   
        public int UserId { get; set; }
        public string UserFullName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;

        public int Id { get; set; }
        public string StoreName { get; set; } = string.Empty;
        public string StoreEmail { get; set; } = string.Empty;
        public string GSTNumber { get; set; } = string.Empty;
        public string PANNumber { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string LogoUrl { get; set; } = string.Empty;

        public VendorStatus Status { get; set; }
    }
}
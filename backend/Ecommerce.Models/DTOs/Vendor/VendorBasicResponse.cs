namespace Ecommerce.Models.DTOs
{
    public class VendorBasicResponse
    {   
        public string StoreName { get; set; } = string.Empty;
        public string StoreEmail { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string LogoUrl { get; set; } = string.Empty;
    }
}
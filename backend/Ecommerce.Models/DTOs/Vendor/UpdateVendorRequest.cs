using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Models.DTOs
{
    public class UpdateVendorRequest
    {
         public string? FullName { get; set; } = null!;

        [RegularExpression( @"^[^@\s]+@[^@\s]+\.(com|in|org)$", ErrorMessage = "Email must be a valid .com address.")]
        [StringLength(100, MinimumLength = 10, ErrorMessage = "Email must be at least 10 characters long.")]
        public string? Email { get; set; } = null!;

        [StringLength(100, MinimumLength = 10, ErrorMessage = "Store Name must be at least 10 characters long.")]
        public string? StoreName { get; set; } = null!;

        [RegularExpression( @"^[^@\s]+@[^@\s]+\.(com|in|org|co)$", ErrorMessage = "Email must be a valid .com address.")]
        [StringLength(100, MinimumLength = 10, ErrorMessage = "Email must be at least 10 characters long.")]
        
        public string? StoreEmail { get; set; }

        [RegularExpression(@"^[A-Z0-9]{15}$", ErrorMessage = "Enter a valid 15 characters GST Number.")]
        public string? GSTNumber { get; set; }

        [RegularExpression(@"^[A-Z0-9]{10}$", ErrorMessage = "PAN Number must be exactly 10 characters.")]
        public string? PANNumber { get; set; }
        
        public string? Description { get; set; }
        
        public string? LogoUrl { get; set; }
    }
}
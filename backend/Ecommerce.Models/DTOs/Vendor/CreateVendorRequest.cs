using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Models.DTOs
{
    public class CreateVendorRequest
    {
        [StringLength(100, MinimumLength = 5, ErrorMessage = "Store Name must be between 5 and 100 characters.")]
        public string StoreName { get; set; } = null!;

        [RegularExpression( @"^[^@\s]+@[^@\s]+\.(com|in|org|co)$", ErrorMessage = "Email must be a valid .com address.")]
        [StringLength(100, MinimumLength = 10, ErrorMessage = "Email must be at least 10 characters long.")]
        public string? StoreEmail { get; set; }

        [Required(ErrorMessage = "GST Number is required.")]
        [RegularExpression(@"^[A-Z0-9]{15}$", ErrorMessage = "Enter a valid 15 characters GST Number.")]
        public string GSTNumber { get; set; } = null!;

        [Required(ErrorMessage = "PAN Number is required.")]
        [RegularExpression(@"^[A-Z0-9]{10}$", ErrorMessage = "PAN Number must be exactly 10 characters.")]
        public string PANNumber { get; set; } = null!;

        public string? Description { get; set; }
        public string? LogoUrl { get; set; }
    }
}
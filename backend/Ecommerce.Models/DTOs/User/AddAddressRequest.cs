using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Models.DTOs
{
    public class AddAddressRequest
    {
        [Required(ErrorMessage = "Recipient name is required.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Recipient name must be between 2 and 100 characters.")]
        public string RecipientName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required.")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone number must contain exactly 10 digits.")]
        public string Phone { get; set; } = string.Empty;  

        [Required(ErrorMessage = "Address Line 1 is required.")]
        [StringLength(250, MinimumLength = 5, ErrorMessage = "Address Line 1 must be between 5 and 250 characters.")]
        public string Line1 { get; set; } = string.Empty;

        [StringLength(150, ErrorMessage = "Address Line 2 cannot exceed 150 characters.")]
        public string? Line2 { get; set; }

        [StringLength(150, ErrorMessage = "Landmark cannot exceed 150 characters.")]
        public string? Landmark { get; set; }          

        [Required(ErrorMessage = "City is required.")]
        [StringLength(100, ErrorMessage = "City name cannot exceed 100 characters.")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "State is required.")]
        [StringLength(100, ErrorMessage = "State name cannot exceed 100 characters.")]
        public string State { get; set; } = string.Empty;

        [Required(ErrorMessage = "Postal code is required.")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "Postal code must contain exactly 6 digits.")]
        public string PostalCode { get; set; } = string.Empty;   

        [Required(ErrorMessage = "Country is required.")]
        [StringLength(50, ErrorMessage = "Country name cannot exceed 50 characters.")]
        public string Country { get; set; } = "India";   

        [StringLength(20, ErrorMessage = "Label cannot exceed 20 characters.")]
        public string? Label { get; set; } 
    }
}
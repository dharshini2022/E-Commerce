using System.ComponentModel.DataAnnotations;
namespace Ecommerce.Models.DTOs
{
    public class UserProfileRequest
    {
        public string? FullName { get; set; } = null!;
        [RegularExpression( @"^[^@\s]+@[^@\s]+\.(com|in|org)$", ErrorMessage = "Email must be a valid .com address.")]
        [StringLength(100, MinimumLength = 10, ErrorMessage = "Email must be at least 10 characters long.")]
        public string? Email { get; set; } = null!;
    }
}

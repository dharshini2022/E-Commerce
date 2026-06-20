using Ecommerce.Models;
using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Models.DTOs
{
    public class RegisterRequest
    {
        [RegularExpression( @"^[^@\s]+@[^@\s]+\.(com|in|org)$", ErrorMessage = "Email must be a valid .com address.")]
        [Required(ErrorMessage = "Email is required")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "FullName is required")]
        public string FullName { get; set; } = null!;
        
        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; } = null!;
        
    }
}
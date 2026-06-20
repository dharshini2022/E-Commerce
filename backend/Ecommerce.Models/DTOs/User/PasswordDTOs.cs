using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Models.DTOs
{
    public class ChangePasswordRequest
    {
        [Required(ErrorMessage = "Old Password is required")]
        public string OldPassword { get; set; } = null!;

        [Required(ErrorMessage = "New Password is required")]
        public string NewPassword { get; set; } = null!;
    }
    public class ResetPasswordRequest
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Email must be a valid email address.")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Token is required.")]
        public string Token { get; set; } = null!;

        [Required(ErrorMessage = "NewPassword is required.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "NewPassword must be at least 6 characters long.")]
        public string NewPassword { get; set; } = null!;
    }
    public class ForgotPasswordRequest
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Email must be a valid email address.")]
        public string Email { get; set; } = null!;
    }
}
using System.ComponentModel.DataAnnotations;
namespace Ecommerce.Models.DTOs
{
    public class LogoutRequest
    {
        [Required(ErrorMessage = "Refresh Token is required")]
        public string RefreshToken { get; set; } = null!;
        [Required(ErrorMessage = "Token Payload has no Id")]
        public int UserId { get; set; }
    }
}
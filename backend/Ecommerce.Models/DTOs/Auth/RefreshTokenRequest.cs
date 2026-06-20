using System.ComponentModel.DataAnnotations;
namespace Ecommerce.Models.DTOs
{
    public class RefreshTokenRequest
    {
        [Required(ErrorMessage = "Refresh Token is required")]
        public string RefreshToken { get; set; } = null!;
    }
}
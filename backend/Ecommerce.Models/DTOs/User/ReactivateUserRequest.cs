using System.ComponentModel.DataAnnotations;
namespace Ecommerce.Models.DTOs
{
    public class ReactivateUserRequest
    {
        [Required(ErrorMessage = "Token Payload has no Id")]
        public int AdminId { get; set; }
        [Required(ErrorMessage = "Target User Id is required")]
        public int UserId { get; set; }
    }
}
using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Models.DTOs
{
    public class SuspendUserRequest
    {
        [Required(ErrorMessage = "Token Payload has Id")]
        public int AdminId { get; set; }
        [Required(ErrorMessage = "Target User Id is required")]
        public int TargetUserId { get; set; }
        [Required(ErrorMessage = "Reason is required")]
        public string Reason { get; set; } = String.Empty;
    }
}
using System.ComponentModel.DataAnnotations;
namespace Ecommerce.Models.DTOs
{
    public class ChangeRoleRequest
    {
        [Required(ErrorMessage = "Target User Id is required")]
        public int UserId { get; set; }
        [Required(ErrorMessage = "New Role is required")]
        public UserRole NewRole { get; set; } 
    }
}
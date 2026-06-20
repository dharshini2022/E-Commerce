using System.ComponentModel.DataAnnotations;
namespace Ecommerce.Models.DTOs
{
    public class UpdateCartItemRequest
    {
        [Required]
        [Range(1, 100, ErrorMessage = "Quantity must be between 1 and 100.")]
        public int NewQuantity { get; set; }
    }
}

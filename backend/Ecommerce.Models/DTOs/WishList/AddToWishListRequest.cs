using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Models.DTOs
{
    public class AddToWishListRequest
    {
        [Required(ErrorMessage = "VariantID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "VariantId must be greater than 0.")]
        public int VariantId { get; set; }
    }
}
using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Models.DTOs
{
    public class AddToCartRequest
    {
        [Required(ErrorMessage = "VariantID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "VariantId must be greater than 0.")]
        public int VariantId { get; set; }

        [Required(ErrorMessage ="Quantity is Required")]
        [Range(1,100,ErrorMessage = "Quanity must be between range 1 to 100")]
        public int Quantity { get; set; }
    }
}
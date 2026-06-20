using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Models.DTOs
{
    public class CreateOrderRequest
    {
        [Required(ErrorMessage = "UserAddressId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "UserAddressId must be a positive integer.")]
        public int UserAddressId { get; set; }
        
        public string? DiscountCode { get; set; }
    }
}
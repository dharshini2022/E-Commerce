using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Models.DTOs
{
    public class AddProductVariantRequest
    {
        [Required(ErrorMessage = "Stock quantity is required.")]
        [Range(0, int.MaxValue, ErrorMessage = "Stock quantity cannot be negative.")]
        public int StockQty { get; set; }

        [Required(ErrorMessage = "Price is required.")]
        [Range(typeof(decimal), "0.01", "999999999.99",ErrorMessage = "Price must be greater than 0.")]
        public decimal Price { get; set; }
        public bool? IsDefault { get; set; }

        [Required(ErrorMessage = "AvailableValues is required.")]
        [MinLength(1, ErrorMessage = "At least one variant attribute is required.")]
        public Dictionary<string, string>? AvailableValues { get; set; } = new();
    }
}
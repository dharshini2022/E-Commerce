using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Models.DTOs
{
    public class UpdateProductVariantRequest
    {
        [Range(typeof(decimal), "0.01", "999999999.99",ErrorMessage = "Price must be greater than 0.")]
        public decimal? Price { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Stock quantity cannot be negative.")]
        public int? StockQty { get; set; }
        
        public Dictionary<string, string>? AvailableValues { get; set; } = new();
    }
}
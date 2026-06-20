using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Models.DTOs
{
    public class CreateDiscountRequest
    {

        [Range(1, int.MaxValue, ErrorMessage = "ProductId must be greater than 0.")]
        public int? ProductId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "CategoryId must be greater than 0.")]
        public int? CategoryId { get; set; }

        [Required(ErrorMessage = "Discount scope is required.")]
        public String Scope { get; set; } = String.Empty;

        [Required(ErrorMessage = "Discount type is required.")]
        public String Type { get; set; } = String.Empty;

        [Range(0.01, double.MaxValue, ErrorMessage = "Discount value must be greater than 0.")]
        public decimal Value { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Minimum order amount cannot be negative.")]
        public decimal MinOrderValue { get; set; }

        [Range(10, int.MaxValue, ErrorMessage = "Usage limit must be at least 10.")]
        public int UsageLimit { get; set; }

        [Required(ErrorMessage = "Expiry date is required.")]
        public DateTime ExpiresAt { get; set; }
    }
}
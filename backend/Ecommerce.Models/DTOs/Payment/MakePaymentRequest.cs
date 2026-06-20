using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Models.DTOs
{
    public class MakePaymentRequest
    {
        [Required(ErrorMessage = "PaymentMethodId is required.")]
        public string PaymentMethodId { get; set; } = null!;
    }
}
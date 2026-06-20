using System;

namespace Ecommerce.Models.DTOs
{
    public class PaymentResponseDTO
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public string TransactionId { get; set; } = null!;
        public string? StripePaymentIntentId { get; set; }
        public string Provider { get; set; } = "Stripe";
        public string Status { get; set; } = null!;
        public DateTime PaidAt { get; set; }
        public int? OrderId { get; set; }
    }
}

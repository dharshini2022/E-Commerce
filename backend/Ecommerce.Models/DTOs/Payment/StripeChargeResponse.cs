namespace Ecommerce.Models.DTOs
{
    public class StripeChargeResponse
    {
        public string PaymentIntentId { get; set; } = null!;
        public string Status { get; set; } = null!;   
        public decimal Amount { get; set; }
    }
}
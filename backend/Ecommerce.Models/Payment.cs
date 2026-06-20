namespace Ecommerce.Models
{
    public enum PaymentStatus
    {
        Pending,
        Paid,
        Failed,
        Refunded
    }

    public class Payment
    {
        public int Id { get; set; }

        public decimal Amount { get; set; }

        public string TransactionId { get; set; } = null!;  

        public string? StripePaymentIntentId { get; set; }   
        public string Provider { get; set; } = "Stripe";

        public PaymentStatus Status { get; set; }

        public DateTime PaidAt { get; set; }
        
       //Relation
        public Order Order { get; set; } = null!;
        public Return Return { get; set; } = null!;
    }
}

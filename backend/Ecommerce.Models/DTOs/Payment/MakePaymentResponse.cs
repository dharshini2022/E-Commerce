namespace Ecommerce.Models.DTOs
{
    public class MakePaymentResponse
    {
        public bool Success { get; set; }
        public int OrderId { get; set; }
        public string Status { get; set; } = null!;     
        public string Message { get; set; } = null!;
        public string? ChargeId { get; set; }           
    }
}
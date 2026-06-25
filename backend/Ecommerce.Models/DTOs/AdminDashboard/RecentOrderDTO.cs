namespace Ecommerce.Models.DTOs
{
    public class RecentOrderDTO
    {
        public int Id { get; set; }
        public string CustomerName { get; set; } = null!;
        public decimal Amount { get; set; }
        public string Status { get; set; } = null!;
    }
}
namespace Ecommerce.Models.DTOs
{
    public class CartItemEvaluationResponse
    {
        public int Id { get; set; }
        public int VendorId { get; set; }
        public int CategoryId {get; set; }
        public int ProductId { get; set; }
        public decimal SubTotal { get; set; }
    }
}

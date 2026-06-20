using System.ComponentModel.DataAnnotations;
namespace Ecommerce.Models.DTOs
{
    public class CartEvaluationRequest
    {
        public decimal SubTotal { get; set; }

        public List<CartItemEvaluationResponse> Items { get; set; } = [];
    }
}
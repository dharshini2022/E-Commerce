using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Models.DTOs
{
    public class ToggleDiscountStatusResponse
    {
        public int Id {get; set;}
        public string Code { get; set; } = null!;
        public bool IsActive {get; set;}
        public DateTime ExpiredAt {get; set;}
    }
}
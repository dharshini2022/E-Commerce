namespace Ecommerce.Models.DTOs
{
    public class OrderStatusDTO
    {
        public int TotalOrders { get; set; }
        public ICollection<DonutSliceDTO> Slices { get; set; } = [];
    }
}
namespace Ecommerce.Models.DTOs
{
    public class DiscountDistributionDTO
    {
        public int TotalTypes { get; set; }
        public ICollection<DonutSliceDTO> Slices { get; set; } = [];
    }
}
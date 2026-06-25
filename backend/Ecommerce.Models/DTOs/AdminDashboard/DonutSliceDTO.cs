namespace Ecommerce.Models.DTOs
{
    public class DonutSliceDTO
    {
        public string Label { get; set; } = null!;
        public string Color { get; set; } = null!;
        public decimal Percentage { get; set; }
    }
}
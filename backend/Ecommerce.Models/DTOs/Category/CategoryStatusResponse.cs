namespace Ecommerce.Models.DTOs
{
    public class CategoryStatusResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Slug { get; set; } = null!;
        public bool IsActive { get; set; } = true;
    }
    
}
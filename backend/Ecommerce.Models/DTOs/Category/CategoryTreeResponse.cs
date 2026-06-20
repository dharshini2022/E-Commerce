namespace Ecommerce.Models.DTOs
{
    public class CategoryTreeResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Slug { get; set; } = null!;
        public int? ParentId { get; set; }
        public int ProductCount { get; set; } = 0;
        public ICollection<CategoryTreeResponse> Children { get; set; } = new List<CategoryTreeResponse>();
    }
}
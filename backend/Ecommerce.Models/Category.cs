namespace Ecommerce.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string slug { get; set; } = null!; 
        public int? ParentId { get; set; }
        public bool isActive { get; set; } = true;

        public Category? Parent { get; set; }
        public ICollection<Category> Children { get; set; } = [];
        public ICollection<Product> Products { get; set; } = [];
    }

}


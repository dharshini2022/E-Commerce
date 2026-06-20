using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Models.DTOs
{
    public class CreateCategoryRequest
    {
        [Required(ErrorMessage = "Category name is required.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters.")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Slug is required.")]
        [StringLength(150, MinimumLength = 2, ErrorMessage = "Slug must be between 2 and 150 characters.")]
        public string Slug { get; set; } = null!;

        public int? ParentId { get; set; }
    }
}
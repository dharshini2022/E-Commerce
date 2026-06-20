using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Models.DTOs
{
    public class CreateProductRequest
    {
        [Required(ErrorMessage = "Category is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid category.")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Product name is required.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Product name must be between 2 and 100 characters.")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Description is required.")]
        [StringLength(5000, ErrorMessage = "Description cannot exceed 5000 characters.")]
        public string Description { get; set; } = null!;
    }
}
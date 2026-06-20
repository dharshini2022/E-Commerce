using System.ComponentModel.DataAnnotations;
namespace Ecommerce.Models.DTOs
{
    public class UpdateCategoryRequest
    {

        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters.")]
        public string? Name { get; set; }

        [StringLength(150, MinimumLength = 2, ErrorMessage = "Slug must be between 2 and 150 characters.")]
        public string? Slug { get; set; }

        public int? ParentId { get; set; }
    }
}
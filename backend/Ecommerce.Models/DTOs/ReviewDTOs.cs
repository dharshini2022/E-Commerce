using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Models.DTOs
{
    public class CreateReviewRequest
    {
        [Required(ErrorMessage = "ProductId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "ProductId must be a positive integer.")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "OrderId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "OrderId must be a positive integer.")]
        public int OrderId { get; set; }

        [Required(ErrorMessage = "Rating is required.")]
        [Range(0.0, 5.0, ErrorMessage = "Rating must be between 0.0 and 5.0.")]
        public decimal Rating { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters.")]
        public string Title { get; set; } = null!;

        [StringLength(1000, ErrorMessage = "Body cannot exceed 1000 characters.")]
        public string? Body { get; set; }
        
        public ICollection<string> ImageUrls { get; set; } = new List<string>();
    }

    public class UpdateReviewRequest
    {
        [Required(ErrorMessage = "Rating is required.")]
        [Range(0.0, 5.0, ErrorMessage = "Rating must be between 0.0 and 5.0.")]
        public decimal Rating { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters.")]
        public string Title { get; set; } = null!;

        [StringLength(1000, ErrorMessage = "Body cannot exceed 1000 characters.")]
        public string? Body { get; set; }
        
        public ICollection<string> ImageUrls { get; set; } = new List<string>();
    }

    public class ReviewDTO
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public int UserId { get; set; }
        public string UserFullName { get; set; } = null!;
        public int OrderId { get; set; }
        public decimal Rating { get; set; }
        public string Title { get; set; } = null!;
        public string? Body { get; set; }
        public DateTime CreatedAt { get; set; }
        public ICollection<string> ReviewImages { get; set; } = new List<string>();
    }
}

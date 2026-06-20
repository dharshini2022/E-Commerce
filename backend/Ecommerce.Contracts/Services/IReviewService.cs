using Ecommerce.Models.DTOs;

namespace Ecommerce.Contracts.Services
{
    public interface IReviewService
    {
        Task<ReviewDTO> AddReview(int userId, CreateReviewRequest request);
        Task<ReviewDTO> UpdateReview(int userId, int reviewId, UpdateReviewRequest request);
        Task<bool> DeleteReview(int userId, int reviewId);
        Task<ICollection<ReviewDTO>> GetUserReviews(int userId);
        Task<ICollection<ReviewDTO>> GetProductReviews(int productId);
        Task<ICollection<ReviewDTO>> GetVendorReviews(int vendorId);
        Task<ICollection<ReviewDTO>> GetAllReviews(); // Admin
    }
}

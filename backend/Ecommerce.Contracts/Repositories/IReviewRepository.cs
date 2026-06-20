using Ecommerce.Models;

namespace Ecommerce.Contracts.Repositories
{
    public interface IReviewRepository : IRepository<int, Review>
    {
        Task<ICollection<Review>> GetReviewsByProductIdAsync(int productId);
        Task<ICollection<Review>> GetReviewsByUserIdAsync(int userId);
        Task<ICollection<Review>> GetReviewsByVendorIdAsync(int vendorId);
        Task<Review?> GetReviewWithDetailsByIdAsync(int reviewId);
        Task<ICollection<Review>> GetAllReviewsWithDetails();
    }
}

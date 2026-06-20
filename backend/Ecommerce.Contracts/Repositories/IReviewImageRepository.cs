using Ecommerce.Models;

namespace Ecommerce.Contracts.Repositories
{
    public interface IReviewImageRepository : IRepository<int, ReviewImage>
    {
        Task<ICollection<ReviewImage>> GetImagesByReviewIdAsync(int reviewId);
        Task<bool> HardDeleteImagesByReviewId(int reviewId);
    }
}

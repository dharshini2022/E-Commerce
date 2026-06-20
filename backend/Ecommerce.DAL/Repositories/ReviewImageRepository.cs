using Ecommerce.Contracts.Repositories;
using Ecommerce.DAL.Context;
using Ecommerce.Models;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.DAL.Repositories
{
    public class ReviewImageRepository : AbstractRepository<int, ReviewImage>, IReviewImageRepository
    {
        private readonly AppDbContext _dbContext;

        public ReviewImageRepository(AppDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ICollection<ReviewImage>> GetImagesByReviewIdAsync(int reviewId)
        {
            return await _dbContext.Set<ReviewImage>()
                .Where(ri => ri.ReviewId == reviewId)
                .OrderBy(ri => ri.ImageOrder)
                .ToListAsync();
        }

        public async Task<bool> HardDeleteImagesByReviewId(int reviewId)
        {
            var images = await GetImagesByReviewIdAsync(reviewId);
            if (!images.Any()) return true;
            _dbContext.Set<ReviewImage>().RemoveRange(images);
            await _dbContext.SaveChangesAsync();
            return true;
        }
    }
}

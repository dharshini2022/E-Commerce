using Ecommerce.Contracts.Repositories;
using Ecommerce.DAL.Context;
using Ecommerce.Models;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.DAL.Repositories
{
    public class ReviewRepository : AbstractRepository<int, Review>, IReviewRepository
    {
        private readonly AppDbContext _dbContext;

        public ReviewRepository(AppDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ICollection<Review>> GetReviewsByProductIdAsync(int productId)
        {
            return await _dbContext.Reviews
                .Include(r => r.User)
                .Include(r => r.Product)
                .Include(r => r.ReviewImages)
                .Where(r => r.ProductId == productId)
                .ToListAsync();
        }

        public async Task<ICollection<Review>> GetReviewsByUserIdAsync(int userId)
        {
            return await _dbContext.Reviews
                .Include(r => r.User)
                .Include(r => r.Product)
                .Include(r => r.ReviewImages)
                .Where(r => r.UserId == userId)
                .ToListAsync();
        }

        public async Task<ICollection<Review>> GetReviewsByVendorIdAsync(int vendorId)
        {
            return await _dbContext.Reviews
                .Include(r => r.User)
                .Include(r => r.Product)
                    .ThenInclude(p => p.Vendor)
                .Include(r => r.ReviewImages)
                .Where(r => r.Product.VendorId == vendorId)
                .ToListAsync();
        }

        public async Task<Review?> GetReviewWithDetailsByIdAsync(int reviewId)
        {
            return await _dbContext.Reviews
                .Include(r => r.User)
                .Include(r => r.Product)
                .Include(r => r.ReviewImages)
                .FirstOrDefaultAsync(r => r.Id == reviewId);
        }

        public async Task<ICollection<Review>> GetAllReviewsWithDetails()
        {
            return await _dbContext.Reviews
                .Include(r => r.User)
                .Include(r => r.Product)
                .Include(r => r.ReviewImages)
                .ToListAsync();
        }
    }
}

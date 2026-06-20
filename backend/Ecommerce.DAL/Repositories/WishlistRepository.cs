using Ecommerce.Contracts.Repositories;
using Ecommerce.DAL.Context;
using Ecommerce.Models;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.DAL.Repositories
{
    public class WishlistRepository : AbstractRepository<int, Wishlist>, IWishlistRepository
    {
        private readonly AppDbContext _dbContext;

        public WishlistRepository(AppDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Wishlist?> GetWishlistByUserId(int userId)
        {
            return await _dbContext.Wishlists
                .Include(w => w.Items)
                    .ThenInclude(wi => wi.Variant)
                        .ThenInclude(v => v.Product)
                .FirstOrDefaultAsync(w => w.UserId == userId);
        }
    }
}

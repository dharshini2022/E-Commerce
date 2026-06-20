using Ecommerce.Contracts.Repositories;
using Ecommerce.DAL.Context;
using Ecommerce.Models;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.DAL.Repositories
{
    public class WishlistItemRepository : AbstractRepository<int, WishlistItem>, IWishlistItemRepository
    {
        private readonly AppDbContext _dbContext;

        public WishlistItemRepository(AppDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ICollection<WishlistItem>> GetItemsByWishlistId(int wishlistId)
        {
            return await _dbContext.WishlistItems
                .Include(wi => wi.Variant)
                    .ThenInclude(v => v.Product)
                .Where(wi => wi.WishlistId == wishlistId)
                .ToListAsync();
        }

        public async Task<WishlistItem?> GetItemByVariant(int wishlistId, int variantId)
        {
            return await _dbContext.WishlistItems
                .FirstOrDefaultAsync(wi => wi.WishlistId == wishlistId && wi.VariantId == variantId);
        }

        public async Task<bool> HardDelete(int wishlistItemId)
        {
            var item = await GetById(wishlistItemId);
            if (item == null) return false;
            await Delete(wishlistItemId);
            return true;
        }
    }
}

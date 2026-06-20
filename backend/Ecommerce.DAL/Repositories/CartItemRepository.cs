using Ecommerce.Contracts.Repositories;
using Ecommerce.DAL.Context;
using Ecommerce.Models;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.DAL.Repositories
{
    public class CartItemRepository : AbstractRepository<int, CartItem>, ICartItemRepository
    {
        private readonly AppDbContext _dbContext;

        public CartItemRepository(AppDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ICollection<CartItem>> GetActiveCartItems(int cartId)
        {
            return await _dbContext.CartItems
                .Include(ci => ci.Variant)
                    .ThenInclude(v => v.Product)
                .Where(ci => ci.CartId == cartId && ci.Variant.IsActive && ci.Variant.StockQty > 0)
                .ToListAsync();
        }

        public async Task<CartItem?> GetCartItemByVariant(int cartId, int variantId)
        {
            return await _dbContext.CartItems.FirstOrDefaultAsync(ci => ci.CartId == cartId && ci.VariantId == variantId);
        }

        public async Task<bool> HardDelete(int cartItemId)
        {
            var item = await GetById(cartItemId);
            if (item == null) return false;
            await Delete(cartItemId);
            return true;
        }
    }
}

using Ecommerce.Contracts.Repositories;
using Ecommerce.DAL.Context;
using Ecommerce.Models;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.DAL.Repositories
{
    public class CartRepository : AbstractRepository<int, Cart>, ICartRepository
    {
        private readonly AppDbContext _dbContext;

        public CartRepository(AppDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Cart?> GetCartByUserId(int userId)
        {
            return await _dbContext.Carts
                .Include(c => c.Items)
                    .ThenInclude(ci => ci.Variant)
                        .ThenInclude(v => v.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);
        }

        public async Task<Cart?> GetCartForCheckout(int userId)
        {
            return await _dbContext.Carts.Include(c => c.Items)
                                        .ThenInclude(ci => ci.Variant)
                                        .ThenInclude(v => v.Product)
                                        .ThenInclude(p => p.Vendor)
                                        .FirstOrDefaultAsync(c => c.UserId == userId);
        }

        public async Task ClearCart(int cartId)
        {
            await _dbContext.CartItems.Where(ci => ci.CartId == cartId).ExecuteDeleteAsync();

            var cart = await _dbContext.Carts.FindAsync(cartId);
            if (cart != null)
            {
                cart.UpdatedAt = DateTime.Now;
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}

using Ecommerce.Models;

namespace Ecommerce.Contracts.Repositories
{
    public interface IWishlistRepository : IRepository<int, Wishlist>
    {
        Task<Wishlist?> GetWishlistByUserId(int userId);
    }
}

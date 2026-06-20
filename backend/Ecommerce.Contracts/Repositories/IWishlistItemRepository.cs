using Ecommerce.Models;

namespace Ecommerce.Contracts.Repositories
{
    public interface IWishlistItemRepository : IRepository<int, WishlistItem>
    {
        Task<ICollection<WishlistItem>> GetItemsByWishlistId(int wishlistId);
        Task<WishlistItem?> GetItemByVariant(int wishlistId, int variantId);
        Task<bool> HardDelete(int wishlistItemId);
    }
}

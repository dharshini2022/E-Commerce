using Ecommerce.Models;

namespace Ecommerce.Contracts.Repositories
{
    public interface ICartItemRepository : IRepository<int, CartItem>
    {
        Task<ICollection<CartItem>> GetActiveCartItems(int cartId);
        Task<CartItem?> GetCartItemByVariant(int cartId, int variantId);
        Task<bool> HardDelete(int cartItemId);
    }
}

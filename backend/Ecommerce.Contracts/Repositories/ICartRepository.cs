using Ecommerce.Models;

namespace Ecommerce.Contracts.Repositories
{
    public interface ICartRepository : IRepository<int, Cart>
    {
        Task<Cart?> GetCartByUserId(int userId);
        Task<Cart?> GetCartForCheckout(int userId);
        Task ClearCart(int cartId);

    }
}

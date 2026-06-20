using Ecommerce.Models;
using Ecommerce.Models.DTOs;

namespace Ecommerce.Contracts.Services
{
    public interface IWishlistService
    {
        Task<Wishlist> GetWishlistByUserId();
        Task<WishListItemResponse> AddToWishlist(AddToWishListRequest request);
        Task<WishListItemResponse> RemoveFromWishlist(int wishListItemId);
    }
}

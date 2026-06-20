using System.Diagnostics.Tracing;
using AutoMapper;
using Ecommerce.Contracts.Repositories;
using Ecommerce.Contracts.Services;
using Ecommerce.Models;
using Ecommerce.Models.DTOs;

namespace Ecommerce.BLL
{
    public class WishlistService : IWishlistService
    {
        private readonly IWishlistRepository _wishlistRepository;
        private readonly IWishlistItemRepository _wishlistItemRepository;
        private readonly IProductVariantRepository _variantRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMapper _mapper;

        public WishlistService(IWishlistRepository wishlistRepository,IWishlistItemRepository wishlistItemRepository,IProductVariantRepository variantRepository, ICurrentUserService currentUserService, IMapper mapper)
        {
            _wishlistRepository = wishlistRepository;
            _wishlistItemRepository = wishlistItemRepository;
            _variantRepository = variantRepository;
            _currentUserService = currentUserService;
            _mapper = mapper;
        }

        public async Task<Wishlist> GetWishlistByUserId()
        {
            int userId = _currentUserService.UserId;
            var wishlist = await _wishlistRepository.GetWishlistByUserId(userId);
            if (wishlist == null)
            {
                wishlist = await _wishlistRepository.Create(new Wishlist { UserId = userId });
            }
            return wishlist;
        }

        public async Task<WishListItemResponse> AddToWishlist(AddToWishListRequest request)
        {
            var wishlist = await GetWishlistByUserId();
            int variantId = request.VariantId;
            var variant = await _variantRepository.GetById(variantId);
            if (variant == null || !variant.IsActive)   throw new KeyNotFoundException($"Variant {variantId} not found or inactive.");

            var existing = await _wishlistItemRepository.GetItemByVariant(wishlist.Id, variantId);
            if(existing != null)    return _mapper.Map<WishListItemResponse>(existing);

            var item = new WishlistItem{WishlistId = wishlist.Id, VariantId = variantId, AddedAt = DateTime.Now};
            var result = await _wishlistItemRepository.Create(item);
            return _mapper.Map<WishListItemResponse>(result);
        }

        public async Task<WishListItemResponse> RemoveFromWishlist(int wishListItemId)
        {
            var wishlist = await GetWishlistByUserId();
            var item = await _wishlistItemRepository.GetById(wishListItemId);
            if (item == null || item.WishlistId != wishlist.Id)
                throw new KeyNotFoundException("Wishlist item not found.");
            await _wishlistItemRepository.HardDelete(wishListItemId);
            return _mapper.Map<WishListItemResponse>(item);
        }
    }
}

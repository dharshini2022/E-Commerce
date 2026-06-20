using AutoMapper;
using Ecommerce.Contracts.Services;
using Ecommerce.Models;
using Ecommerce.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Customer")]
    public class WishlistController : ControllerBase
    {
        private readonly IWishlistService _wishlistService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMapper _mapper;

        public WishlistController(IWishlistService wishlistService, ICurrentUserService currentUserService, IMapper mapper)
        {
            _wishlistService = wishlistService;
            _currentUserService = currentUserService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetWishList()
        {
            var wishlist = await _wishlistService.GetWishlistByUserId();
            var response = new WishListResponse
            {
                Id = wishlist.Id,
                UserId = wishlist.UserId,
                Items = _mapper.Map<List<WishListItemResponse>>(wishlist.Items)
            };
            response.TotalItems = response.Items.Count();

            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> AddToWishList([FromBody] AddToWishListRequest request)
        {
            var wishlistItem = await _wishlistService.AddToWishlist(request);
            return Ok(new {message = "Item added to WishLists successfully", data = wishlistItem});
        }

        [HttpDelete("{wishListItemId}")]
        public async Task<IActionResult> RemoveFromWishList([FromRoute] int wishListItemId)
        {
            var result = await _wishlistService.RemoveFromWishlist(wishListItemId);

            return Ok(new {message = "Item removed from Wishlist", date = result});
        }
    }
}
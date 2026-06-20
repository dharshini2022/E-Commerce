using AutoMapper;
using Ecommerce.Contracts.Services;
using Ecommerce.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Customer")]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;
        private readonly ICurrentUserService _currentUser;
        private readonly IMapper _mapper;

        public CartController(ICartService cartService, ICurrentUserService currentUser, IMapper mapper)
        {
            _cartService = cartService;
            _currentUser = currentUser;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            var cart = await _cartService.GetOrCreateCart();

            var response = new CartResponse
            {
                Id = cart.Id,
                UserId = cart.UserId,
                UpdatedAt = cart.UpdatedAt,
                Items = _mapper.Map<List<CartItemResponse>>(cart.Items)
            };

            response.TotalItems = response.Items.Sum(i => i.Quantity);
            response.TotalAmount = response.Items.Sum(i => i.SubTotal);

            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
        {
            var cartItem = await _cartService.AddToCart(request);

            return Ok(new { message = "Item added to cart successfully.", data = cartItem });
        }


        [HttpDelete("{cartItemId}")]
        public async Task<IActionResult> RemoveFromCart([FromRoute] int cartItemId)
        {
            var result = await _cartService.RemoveFromCart(cartItemId);
            return Ok(new { message = "Item removed from cart.", data = result });
        }

        [HttpPatch("{cartItemId}")]
        public async Task<IActionResult> UpdateCartItemQuantity([FromRoute] int cartItemId, [FromBody] UpdateCartItemRequest request)
        {
            var cartItem = await _cartService.UpdateCartItemQuantity(cartItemId, request);

            if (request.NewQuantity <= 0)   return Ok(new { message = "Item removed from cart as quantity reached zero." });

            return Ok(new { message = "Cart item quantity updated.", data = cartItem });
        }
    }
}

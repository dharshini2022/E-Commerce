using Ecommerce.Contracts.Repositories;
using Ecommerce.Contracts.Services;
using Ecommerce.Shared.Exceptions;
using Ecommerce.Models;
using Ecommerce.Models.DTOs;
using AutoMapper;
using Ecommerce.DAL.Context;

namespace Ecommerce.BLL
{
    public class CartService : ICartService
    {
        private readonly AppDbContext _dbContext;
        private readonly ICartRepository _cartRepository;
        private readonly ICartItemRepository _cartItemRepository;
        private readonly IProductVariantRepository _variantRepository;
        private readonly ICurrentUserService _currentUser;
        private readonly IMapper _mapper;

        public CartService(AppDbContext dbContext, ICartRepository cartRepository,ICartItemRepository cartItemRepository,IProductVariantRepository variantRepository,ICurrentUserService currentUser, IMapper mapper)
        {
            _dbContext = dbContext;
            _cartRepository = cartRepository;
            _cartItemRepository = cartItemRepository;
            _variantRepository = variantRepository;
            _currentUser = currentUser;
            _mapper = mapper;
        }
        
        public async Task<Cart> GetOrCreateCart()
        {
            int userId = _currentUser.UserId;
            var cart = await _cartRepository.GetCartByUserId(userId);
            if (cart == null)
            {
                cart = await _cartRepository.Create(new Cart { UserId = userId, UpdatedAt = DateTime.Now });
            }
            return cart;
        }

        public async Task<List<CartItem>> GetEligibleItems(int userId)
        {
            var cart = await _cartRepository.GetCartByUserId(userId)    ?? throw new KeyNotFoundException("Cart not found.");

            if (!cart.Items.Any())  throw new ValidationException("Cart is empty.");

            var eligibleItems = cart.Items.Where(ci => ci.Variant.IsActive && (ci.Variant.StockQty - ci.Variant.ReservedStockQty) >= ci.Quantity).ToList();

            if (!eligibleItems.Any())   throw new ValidationException("None of the cart items are available.");

            return eligibleItems;
        }

        public async Task<CartItemResponse> AddToCart(AddToCartRequest request)
        {
            int userId = _currentUser.UserId;
            if (request.Quantity <= 0)  throw new ValidationException("Quantity must be greater than zero.");

            var variant = await _variantRepository.GetById(request.VariantId) ?? throw new KeyNotFoundException($"Product variant with ID {request.VariantId} not found.");

            if (!variant.IsActive) throw new InvalidOperationException($"Variant {request.VariantId} is no longer available.");

            int availableStock = variant.StockQty - variant.ReservedStockQty;
            if (availableStock < request.Quantity) throw new InsufficientStockException($"Insufficient stock. Available: {availableStock}, Requested: {request.Quantity}.");

            var cart = await GetOrCreateCart();

            var existingItem = await _cartItemRepository.GetCartItemByVariant(cart.Id, variant.Id);
            if (existingItem != null)
            {
                return await UpdateCartItemQuantity(existingItem.Id, new UpdateCartItemRequest { NewQuantity = existingItem.Quantity + request.Quantity });
            }

            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var cartItem = new CartItem{CartId = cart.Id, VariantId = variant.Id, Quantity = request.Quantity};

                cart.UpdatedAt = DateTime.Now;
                await _cartRepository.Update(cart.Id, cart);
                var result =  await _cartItemRepository.Create(cartItem);
                await transaction.CommitAsync();

                return _mapper.Map<CartItemResponse>(result);
            }
            catch(Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<CartItemDeletionResponse> RemoveFromCart(int cartItemId)
        {
            int userId = _currentUser.UserId;
            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var item = await _cartItemRepository.GetById(cartItemId) ?? throw new KeyNotFoundException($"Cart item with ID {cartItemId} not found.");
                await HandleCartItemDeletion(item);
                await transaction.CommitAsync();
                return _mapper.Map<CartItemDeletionResponse>(item);
                
            }
            catch(Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task ClearCart()
        {
            int userId = _currentUser.UserId;
            var cart = await GetOrCreateCart();
            await _cartRepository.ClearCart(cart.Id);
        }


    
       public async Task<CartItemResponse> UpdateCartItemQuantity(int cartItemId, UpdateCartItemRequest request)
        {
            int userId = _currentUser.UserId;
            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var cart = await GetOrCreateCart();
                var item = await _cartItemRepository.GetById(cartItemId) ?? throw new KeyNotFoundException($"Cart item with ID {cartItemId} not found.");

                ValidateCartOwnership(item, cart.Id);

                if (request.NewQuantity <= 0)
                {
                    await HandleCartItemDeletion(item);
                }
                else
                {
                    await HandleStockAndQuantityUpdate(item, request.NewQuantity);
                }

                cart.UpdatedAt = DateTime.Now; 
                await _cartRepository.Update(cart.Id, cart);
                await transaction.CommitAsync();

                return _mapper.Map<CartItemResponse>(item);
            }
            catch(Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private void ValidateCartOwnership(CartItem item, int cartId)
        {
            if (item.CartId != cartId)
            {
                throw new UnauthorizedAccessException("This cart item does not belong to your cart.");
            }
        }

        private async Task HandleCartItemDeletion(CartItem item)
        {
            await _cartItemRepository.HardDelete(item.Id);
            item.Quantity = 0; 
        }

        private async Task HandleStockAndQuantityUpdate(CartItem item, int newQuantity)
        {
            var variant = await _variantRepository.GetById(item.VariantId);
            int difference = newQuantity - item.Quantity;

            if (difference > 0)
            {
                int availableStock = variant!.StockQty - variant.ReservedStockQty;
                if (availableStock < difference)
                {
                    throw new InsufficientStockException($"Insufficient stock. Available: {availableStock}, Additional requested: {difference}.");
                }
            }
        
            item.Quantity = newQuantity;
            await _cartItemRepository.Update(item.Id, item);
        }
    }
}

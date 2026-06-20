using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Ecommerce.BLL;
using Ecommerce.Contracts.Repositories;
using Ecommerce.Contracts.Services;
using Ecommerce.DAL.Context;
using Ecommerce.Models;
using Ecommerce.Models.DTOs;
using Ecommerce.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using NUnit.Framework;

namespace Ecommerce.Test
{
    [TestFixture]
    public class CartServiceTest
    {
        private Mock<AppDbContext> _mockDbContext;
        private Mock<DatabaseFacade> _mockDatabaseFacade;
        private Mock<IDbContextTransaction> _mockTransaction;
        private Mock<ICartRepository> _mockCartRepo;
        private Mock<ICartItemRepository> _mockCartItemRepo;
        private Mock<IProductVariantRepository> _mockVariantRepo;
        private Mock<ICurrentUserService> _mockCurrentUser;
        private Mock<IMapper> _mockMapper;
        private CartService _cartService;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _mockDbContext = new Mock<AppDbContext>(options);
            _mockDatabaseFacade = new Mock<DatabaseFacade>(_mockDbContext.Object);
            _mockTransaction = new Mock<IDbContextTransaction>();

            _mockDbContext.Setup(x => x.Database).Returns(_mockDatabaseFacade.Object);
            _mockDatabaseFacade.Setup(d => d.BeginTransactionAsync(It.IsAny<CancellationToken>()))
                               .ReturnsAsync(_mockTransaction.Object);

            _mockCartRepo = new Mock<ICartRepository>();
            _mockCartItemRepo = new Mock<ICartItemRepository>();
            _mockVariantRepo = new Mock<IProductVariantRepository>();
            _mockCurrentUser = new Mock<ICurrentUserService>();
            _mockMapper = new Mock<IMapper>();

            _cartService = new CartService(
                _mockDbContext.Object,
                _mockCartRepo.Object,
                _mockCartItemRepo.Object,
                _mockVariantRepo.Object,
                _mockCurrentUser.Object,
                _mockMapper.Object
            );
        }

        [Test]
        public async Task GetOrCreateCart_ShouldCreateCart_WhenCartDoesNotExist()
        {
            _mockCurrentUser.Setup(u => u.UserId).Returns(1);
            _mockCartRepo.Setup(r => r.GetCartByUserId(1)).ReturnsAsync((Cart?)null);
            _mockCartRepo.Setup(r => r.Create(It.IsAny<Cart>())).ReturnsAsync(new Cart { Id = 100, UserId = 1 });

            var result = await _cartService.GetOrCreateCart();

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(100));
            _mockCartRepo.Verify(r => r.Create(It.IsAny<Cart>()), Times.Once);
        }

        [Test]
        public async Task GetEligibleItems_ShouldThrowKeyNotFoundException_WhenCartDoesNotExist()
        {
            _mockCartRepo.Setup(r => r.GetCartByUserId(1)).ReturnsAsync((Cart?)null);

            Assert.ThrowsAsync<KeyNotFoundException>(async () => await _cartService.GetEligibleItems(1));
        }

        [Test]
        public async Task GetEligibleItems_ShouldThrowValidationException_WhenCartIsEmpty()
        {
            var cart = new Cart { Id = 100, Items = new List<CartItem>() };
            _mockCartRepo.Setup(r => r.GetCartByUserId(1)).ReturnsAsync(cart);

            var ex = Assert.ThrowsAsync<ValidationException>(async () => await _cartService.GetEligibleItems(1));
            Assert.That(ex.Message, Is.EqualTo("Cart is empty."));
        }

        [Test]
        public async Task GetEligibleItems_ShouldThrowValidationException_WhenNoItemsAreEligible()
        {
            var cart = new Cart
            {
                Id = 100,
                Items = new List<CartItem>
                {
                    new CartItem 
                    { 
                        Quantity = 5,
                        Variant = new ProductVariant { IsActive = false, StockQty = 10, ReservedStockQty = 0 }
                    }
                }
            };
            _mockCartRepo.Setup(r => r.GetCartByUserId(1)).ReturnsAsync(cart);

            var ex = Assert.ThrowsAsync<ValidationException>(async () => await _cartService.GetEligibleItems(1));
            Assert.That(ex.Message, Is.EqualTo("None of the cart items are available."));
        }

        [Test]
        public async Task GetEligibleItems_ShouldReturnEligibleItems_WhenValid()
        {
            var eligibleItem = new CartItem 
            { 
                Quantity = 2,
                Variant = new ProductVariant { IsActive = true, StockQty = 10, ReservedStockQty = 0 }
            };
            var cart = new Cart
            {
                Id = 100,
                Items = new List<CartItem> { eligibleItem }
            };
            _mockCartRepo.Setup(r => r.GetCartByUserId(1)).ReturnsAsync(cart);

            var result = await _cartService.GetEligibleItems(1);

            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result.First(), Is.EqualTo(eligibleItem));
        }

        [Test]
        public void AddToCart_ShouldThrowValidationException_WhenQuantityIsLessThanOrEqualToZero()
        {
            var request = new AddToCartRequest { VariantId = 10, Quantity = 0 };

            Assert.ThrowsAsync<ValidationException>(async () => await _cartService.AddToCart(request));
        }

        [Test]
        public void AddToCart_ShouldThrowKeyNotFoundException_WhenVariantNotFound()
        {
            _mockVariantRepo.Setup(r => r.GetById(10)).ReturnsAsync((ProductVariant?)null);
            var request = new AddToCartRequest { VariantId = 10, Quantity = 2 };

            Assert.ThrowsAsync<KeyNotFoundException>(async () => await _cartService.AddToCart(request));
        }

        [Test]
        public void AddToCart_ShouldThrowInvalidOperationException_WhenVariantIsNotActive()
        {
            var variant = new ProductVariant { Id = 10, IsActive = false };
            _mockVariantRepo.Setup(r => r.GetById(10)).ReturnsAsync(variant);
            var request = new AddToCartRequest { VariantId = 10, Quantity = 2 };

            Assert.ThrowsAsync<InvalidOperationException>(async () => await _cartService.AddToCart(request));
        }

        [Test]
        public void AddToCart_ShouldThrowInsufficientStockException_WhenStockIsInsufficient()
        {
            var variant = new ProductVariant { Id = 10, IsActive = true, StockQty = 5, ReservedStockQty = 4 };
            _mockVariantRepo.Setup(r => r.GetById(10)).ReturnsAsync(variant);
            var request = new AddToCartRequest { VariantId = 10, Quantity = 2 };

            Assert.ThrowsAsync<InsufficientStockException>(async () => await _cartService.AddToCart(request));
        }

        [Test]
        public async Task AddToCart_ExistingItem_CreatesItemAndUsesTransaction()
        {
            _mockCurrentUser.Setup(u => u.UserId).Returns(1);
            var request = new AddToCartRequest { VariantId = 10, Quantity = 2 };

            var variant = new ProductVariant { Id = 10, StockQty = 5, IsActive = true };
            _mockVariantRepo.Setup(r => r.GetById(10)).ReturnsAsync(variant);

            var cart = new Cart { Id = 100, UserId = 1 };
            _mockCartRepo.Setup(r => r.GetCartByUserId(1)).ReturnsAsync(cart);

            _mockCartItemRepo.Setup(r => r.GetCartItemByVariant(100, 10)).ReturnsAsync((CartItem?)null);

            var createdItem = new CartItem { Id = 500, CartId = 100, VariantId = 10, Quantity = 2 };
            _mockCartItemRepo.Setup(r => r.Create(It.IsAny<CartItem>())).ReturnsAsync(createdItem);

            var expectedResponse = new CartItemResponse { Id = 500, Quantity = 2, VariantId = 10 };
            _mockMapper.Setup(m => m.Map<CartItemResponse>(It.IsAny<CartItem>())).Returns(expectedResponse);

            var result = await _cartService.AddToCart(request);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(500));
            
            _mockDatabaseFacade.Verify(d => d.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
            _mockTransaction.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);

            _mockCartRepo.Verify(r => r.Update(100, cart), Times.Once);
            _mockCartItemRepo.Verify(r => r.Create(It.IsAny<CartItem>()), Times.Once);
        }

        [Test]
        public async Task AddToCart_ExistingItem_UpdatesQuantity()
        {
            _mockCurrentUser.Setup(u => u.UserId).Returns(1);
            var request = new AddToCartRequest { VariantId = 10, Quantity = 2 };

            var variant = new ProductVariant { Id = 10, StockQty = 10, IsActive = true };
            _mockVariantRepo.Setup(r => r.GetById(10)).ReturnsAsync(variant);

            var cart = new Cart { Id = 100, UserId = 1 };
            _mockCartRepo.Setup(r => r.GetCartByUserId(1)).ReturnsAsync(cart);

            var existingItem = new CartItem { Id = 500, CartId = 100, VariantId = 10, Quantity = 3 };
            _mockCartItemRepo.Setup(r => r.GetCartItemByVariant(100, 10)).ReturnsAsync(existingItem);
            _mockCartItemRepo.Setup(r => r.GetById(500)).ReturnsAsync(existingItem);

            var expectedResponse = new CartItemResponse { Id = 500, Quantity = 5, VariantId = 10 };
            _mockMapper.Setup(m => m.Map<CartItemResponse>(It.IsAny<CartItem>())).Returns(expectedResponse);

            var result = await _cartService.AddToCart(request);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Quantity, Is.EqualTo(5));
            _mockCartItemRepo.Verify(r => r.Update(500, existingItem), Times.Once);
        }

        [Test]
        public void AddToCart_ShouldRollbackAndThrow_WhenTransactionFails()
        {
            _mockCurrentUser.Setup(u => u.UserId).Returns(1);
            var request = new AddToCartRequest { VariantId = 10, Quantity = 2 };

            var variant = new ProductVariant { Id = 10, StockQty = 10, IsActive = true };
            _mockVariantRepo.Setup(r => r.GetById(10)).ReturnsAsync(variant);

            var cart = new Cart { Id = 100, UserId = 1 };
            _mockCartRepo.Setup(r => r.GetCartByUserId(1)).ReturnsAsync(cart);
            _mockCartItemRepo.Setup(r => r.GetCartItemByVariant(100, 10)).ReturnsAsync((CartItem?)null);

            _mockCartItemRepo.Setup(r => r.Create(It.IsAny<CartItem>())).ThrowsAsync(new Exception("DB Error"));

            Assert.ThrowsAsync<Exception>(async () => await _cartService.AddToCart(request));
            _mockTransaction.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public void RemoveFromCart_ShouldThrowKeyNotFoundException_WhenItemDoesNotExist()
        {
            _mockCurrentUser.Setup(u => u.UserId).Returns(1);
            _mockCartItemRepo.Setup(r => r.GetById(500)).ReturnsAsync((CartItem?)null);

            Assert.ThrowsAsync<KeyNotFoundException>(async () => await _cartService.RemoveFromCart(500));
            _mockTransaction.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task RemoveFromCart_CallsUpdateCartItemQuantityWithZero()
        {
            _mockCurrentUser.Setup(u => u.UserId).Returns(1);
            var cart = new Cart { Id = 100, UserId = 1 };
            _mockCartRepo.Setup(r => r.GetCartByUserId(1)).ReturnsAsync(cart);

            var item = new CartItem { Id = 500, CartId = 100, VariantId = 10, Quantity = 2 };
            _mockCartItemRepo.Setup(r => r.GetById(500)).ReturnsAsync(item);

            _mockCartItemRepo.Setup(r => r.HardDelete(500)).ReturnsAsync(true);

            var expectedResponse = new CartItemDeletionResponse { ProductName = "Test Product", UnitPrice = 15.0m };
            _mockMapper.Setup(m => m.Map<CartItemDeletionResponse>(It.IsAny<CartItem>())).Returns(expectedResponse);

            var result = await _cartService.RemoveFromCart(500);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.ProductName, Is.EqualTo("Test Product"));
            
            _mockCartItemRepo.Verify(r => r.HardDelete(500), Times.Once);
            _mockTransaction.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task ClearCart_ShouldInvokeRepositoryClear()
        {
            _mockCurrentUser.Setup(u => u.UserId).Returns(1);
            var cart = new Cart { Id = 100, UserId = 1 };
            _mockCartRepo.Setup(r => r.GetCartByUserId(1)).ReturnsAsync(cart);
            _mockCartRepo.Setup(r => r.ClearCart(100)).Returns(Task.CompletedTask);

            await _cartService.ClearCart();

            _mockCartRepo.Verify(r => r.ClearCart(100), Times.Once);
        }

        [Test]
        public void UpdateCartItemQuantity_ShouldThrowUnauthorizedAccessException_WhenMismatchedCartId()
        {
            _mockCurrentUser.Setup(u => u.UserId).Returns(1);
            var cart = new Cart { Id = 100, UserId = 1 };
            _mockCartRepo.Setup(r => r.GetCartByUserId(1)).ReturnsAsync(cart);

            var item = new CartItem { Id = 500, CartId = 200, VariantId = 10, Quantity = 2 };
            _mockCartItemRepo.Setup(r => r.GetById(500)).ReturnsAsync(item);

            Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await _cartService.UpdateCartItemQuantity(500, new UpdateCartItemRequest { NewQuantity = 5 }));
            _mockTransaction.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public void UpdateCartItemQuantity_ShouldThrowInsufficientStockException_WhenIncreasingAndStockMismatched()
        {
            _mockCurrentUser.Setup(u => u.UserId).Returns(1);
            var cart = new Cart { Id = 100, UserId = 1 };
            _mockCartRepo.Setup(r => r.GetCartByUserId(1)).ReturnsAsync(cart);

            var item = new CartItem { Id = 500, CartId = 100, VariantId = 10, Quantity = 2 };
            _mockCartItemRepo.Setup(r => r.GetById(500)).ReturnsAsync(item);

            var variant = new ProductVariant { Id = 10, StockQty = 3, ReservedStockQty = 2 };
            _mockVariantRepo.Setup(r => r.GetById(10)).ReturnsAsync(variant);

            Assert.ThrowsAsync<InsufficientStockException>(async () => await _cartService.UpdateCartItemQuantity(500, new UpdateCartItemRequest { NewQuantity = 5 }));
        }

        [Test]
        public async Task UpdateCartItemQuantity_ShouldRemoveItem_WhenNewQuantityIsZeroOrNegative()
        {
            _mockCurrentUser.Setup(u => u.UserId).Returns(1);
            var cart = new Cart { Id = 100, UserId = 1 };
            _mockCartRepo.Setup(r => r.GetCartByUserId(1)).ReturnsAsync(cart);

            var item = new CartItem { Id = 500, CartId = 100, VariantId = 10, Quantity = 2 };
            _mockCartItemRepo.Setup(r => r.GetById(500)).ReturnsAsync(item);

            _mockCartItemRepo.Setup(r => r.HardDelete(500)).ReturnsAsync(true);
            _mockCartRepo.Setup(r => r.Update(100, cart)).ReturnsAsync(cart);

            var expectedResponse = new CartItemResponse { Id = 500 };
            _mockMapper.Setup(m => m.Map<CartItemResponse>(item)).Returns(expectedResponse);

            var result = await _cartService.UpdateCartItemQuantity(500, new UpdateCartItemRequest { NewQuantity = 0 });

            Assert.That(result, Is.Not.Null);
            _mockCartItemRepo.Verify(r => r.HardDelete(500), Times.Once);
            _mockTransaction.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}

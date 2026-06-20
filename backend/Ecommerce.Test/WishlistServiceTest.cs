using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Ecommerce.BLL;
using Ecommerce.Contracts.Repositories;
using Ecommerce.Contracts.Services;
using Ecommerce.Models;
using Ecommerce.Models.DTOs;
using Moq;
using NUnit.Framework;

namespace Ecommerce.Test
{
    [TestFixture]
    public class WishlistServiceTest
    {
        private Mock<IWishlistRepository> _mockWishlistRepo;
        private Mock<IWishlistItemRepository> _mockWishlistItemRepo;
        private Mock<IProductVariantRepository> _mockVariantRepo;
        private Mock<ICurrentUserService> _mockCurrentUser;
        private Mock<IMapper> _mockMapper;
        private WishlistService _wishlistService;

        [SetUp]
        public void Setup()
        {
            _mockWishlistRepo = new Mock<IWishlistRepository>();
            _mockWishlistItemRepo = new Mock<IWishlistItemRepository>();
            _mockVariantRepo = new Mock<IProductVariantRepository>();
            _mockCurrentUser = new Mock<ICurrentUserService>();
            _mockMapper = new Mock<IMapper>();

            _wishlistService = new WishlistService(
                _mockWishlistRepo.Object,
                _mockWishlistItemRepo.Object,
                _mockVariantRepo.Object,
                _mockCurrentUser.Object,
                _mockMapper.Object
            );
        }

        [Test]
        public async Task GetWishlistByUserId_ShouldCreate_WhenNotFound()
        {
           
            _mockCurrentUser.Setup(u => u.UserId).Returns(1);
            _mockWishlistRepo.Setup(r => r.GetWishlistByUserId(1)).ReturnsAsync((Wishlist?)null);
            _mockWishlistRepo.Setup(r => r.Create(It.IsAny<Wishlist>())).ReturnsAsync(new Wishlist { Id = 10 });

            
            var result = await _wishlistService.GetWishlistByUserId();

            
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(10));
            _mockWishlistRepo.Verify(r => r.Create(It.Is<Wishlist>(w => w.UserId == 1)), Times.Once);
        }

        [Test]
        public void AddToWishlist_ShouldThrowException_WhenVariantNotFoundOrInactive()
        {
           
            _mockCurrentUser.Setup(u => u.UserId).Returns(1);
            _mockWishlistRepo.Setup(r => r.GetWishlistByUserId(1)).ReturnsAsync(new Wishlist { Id = 10 });
            _mockVariantRepo.Setup(r => r.GetById(5)).ReturnsAsync((ProductVariant?)null);
            var request = new AddToWishListRequest { VariantId = 5 };

            Assert.ThrowsAsync<KeyNotFoundException>(async () => await _wishlistService.AddToWishlist(request));
        }

        [Test]
        public async Task AddToWishlist_ShouldReturnExisting_WhenItemAlreadyInWishlist()
        {
           
            _mockCurrentUser.Setup(u => u.UserId).Returns(1);
            _mockWishlistRepo.Setup(r => r.GetWishlistByUserId(1)).ReturnsAsync(new Wishlist { Id = 10 });
            _mockVariantRepo.Setup(r => r.GetById(5)).ReturnsAsync(new ProductVariant { Id = 5, IsActive = true });
            
            var existing = new WishlistItem { Id = 100 };
            _mockWishlistItemRepo.Setup(r => r.GetItemByVariant(10, 5)).ReturnsAsync(existing);
            _mockMapper.Setup(m => m.Map<WishListItemResponse>(existing)).Returns(new WishListItemResponse { Id = 100 });

            var request = new AddToWishListRequest { VariantId = 5 };

            
            var result = await _wishlistService.AddToWishlist(request);

            
            Assert.That(result.Id, Is.EqualTo(100));
            _mockWishlistItemRepo.Verify(r => r.Create(It.IsAny<WishlistItem>()), Times.Never);
        }

        [Test]
        public async Task AddToWishlist_ShouldCreateItem_WhenNotExists()
        {
           
            _mockCurrentUser.Setup(u => u.UserId).Returns(1);
            _mockWishlistRepo.Setup(r => r.GetWishlistByUserId(1)).ReturnsAsync(new Wishlist { Id = 10 });
            _mockVariantRepo.Setup(r => r.GetById(5)).ReturnsAsync(new ProductVariant { Id = 5, IsActive = true });
            _mockWishlistItemRepo.Setup(r => r.GetItemByVariant(10, 5)).ReturnsAsync((WishlistItem?)null);

            var created = new WishlistItem { Id = 100 };
            _mockWishlistItemRepo.Setup(r => r.Create(It.IsAny<WishlistItem>())).ReturnsAsync(created);
            _mockMapper.Setup(m => m.Map<WishListItemResponse>(created)).Returns(new WishListItemResponse { Id = 100 });

            var request = new AddToWishListRequest { VariantId = 5 };

            
            var result = await _wishlistService.AddToWishlist(request);

            
            Assert.That(result.Id, Is.EqualTo(100));
            _mockWishlistItemRepo.Verify(r => r.Create(It.Is<WishlistItem>(i => i.WishlistId == 10 && i.VariantId == 5)), Times.Once);
        }

        [Test]
        public void RemoveFromWishlist_ShouldThrowException_WhenItemNotFoundOrMismatched()
        {
           
            _mockCurrentUser.Setup(u => u.UserId).Returns(1);
            _mockWishlistRepo.Setup(r => r.GetWishlistByUserId(1)).ReturnsAsync(new Wishlist { Id = 10 });
            
            _mockWishlistItemRepo.Setup(r => r.GetById(100)).ReturnsAsync((WishlistItem?)null);

            _mockWishlistItemRepo.Setup(r => r.GetById(200)).ReturnsAsync(new WishlistItem { WishlistId = 20 });

            Assert.ThrowsAsync<KeyNotFoundException>(async () => await _wishlistService.RemoveFromWishlist(100));
            Assert.ThrowsAsync<KeyNotFoundException>(async () => await _wishlistService.RemoveFromWishlist(200));
        }

        [Test]
        public async Task RemoveFromWishlist_ShouldDelete_WhenValid()
        {
           
            _mockCurrentUser.Setup(u => u.UserId).Returns(1);
            _mockWishlistRepo.Setup(r => r.GetWishlistByUserId(1)).ReturnsAsync(new Wishlist { Id = 10 });
            
            var item = new WishlistItem { Id = 100, WishlistId = 10 };
            _mockWishlistItemRepo.Setup(r => r.GetById(100)).ReturnsAsync(item);
            _mockWishlistItemRepo.Setup(r => r.HardDelete(100)).ReturnsAsync(true);
            _mockMapper.Setup(m => m.Map<WishListItemResponse>(item)).Returns(new WishListItemResponse { Id = 100 });

            
            var result = await _wishlistService.RemoveFromWishlist(100);

            
            Assert.That(result.Id, Is.EqualTo(100));
            _mockWishlistItemRepo.Verify(r => r.HardDelete(100), Times.Once);
        }
    }
}

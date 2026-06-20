using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
    public class DiscountServiceTest
    {
        private Mock<IDiscountRepository> _mockDiscountRepo;
        private Mock<IVendorService> _mockVendorService;
        private Mock<ICategoryService> _mockCategoryService;
        private Mock<IProductService> _mockProductService;
        private Mock<ICurrentUserService> _mockCurrentUserService;
        private Mock<IMapper> _mockMapper;
        private DiscountService _discountService;

        [SetUp]
        public void Setup()
        {
            _mockDiscountRepo = new Mock<IDiscountRepository>();
            _mockVendorService = new Mock<IVendorService>();
            _mockCategoryService = new Mock<ICategoryService>();
            _mockProductService = new Mock<IProductService>();
            _mockCurrentUserService = new Mock<ICurrentUserService>();
            _mockMapper = new Mock<IMapper>();

            _discountService = new DiscountService(
                _mockDiscountRepo.Object,
                _mockVendorService.Object,
                _mockCategoryService.Object,
                _mockProductService.Object,
                _mockCurrentUserService.Object,
                _mockMapper.Object
            );
        }

        [Test]
        public void CreateDiscount_InvalidScope_ThrowsValidationException()
        {
            var request = new CreateDiscountRequest
            {
                Scope = "invalid_scope",
                Type = "percentage",
                Value = 10,
                MinOrderValue = 0,
                UsageLimit = 10,
                ExpiresAt = DateTime.Now.AddDays(1)
            };

            var ex = Assert.ThrowsAsync<ValidationException>(async () =>
                await _discountService.CreateDiscount(request));
            Assert.That(ex.Message, Contains.Substring("Unsupported discount scope"));
        }

        [Test]
        public void CreateDiscount_InvalidType_ThrowsValidationException()
        {
            var request = new CreateDiscountRequest
            {
                Scope = "common",
                Type = "invalid_type",
                Value = 10,
                MinOrderValue = 0,
                UsageLimit = 10,
                ExpiresAt = DateTime.Now.AddDays(1)
            };

            var ex = Assert.ThrowsAsync<ValidationException>(async () =>
                await _discountService.CreateDiscount(request));
            Assert.That(ex.Message, Contains.Substring("Unsupported discount type"));
        }

        [Test]
        public void CreateDiscount_FlatDiscountExceedsMinOrderValue_ThrowsValidationException()
        {
            var request = new CreateDiscountRequest
            {
                Scope = "common",
                Type = "flat",
                Value = 50,
                MinOrderValue = 10,
                UsageLimit = 10,
                ExpiresAt = DateTime.Now.AddDays(1)
            };

            var ex = Assert.ThrowsAsync<ValidationException>(async () =>
                await _discountService.CreateDiscount(request));
            Assert.That(ex.Message, Contains.Substring("Flat discount value cannot exceed minimum order value"));
        }

        [Test]
        public void CreateDiscount_PercentageDiscountExceeds100_ThrowsValidationException()
        {
            var request = new CreateDiscountRequest
            {
                Scope = "common",
                Type = "percentage",
                Value = 101,
                MinOrderValue = 0,
                UsageLimit = 10,
                ExpiresAt = DateTime.Now.AddDays(1)
            };

            var ex = Assert.ThrowsAsync<ValidationException>(async () =>
                await _discountService.CreateDiscount(request));
            Assert.That(ex.Message, Contains.Substring("Percentage discount can't exceed 100"));
        }

        [Test]
        public void CreateDiscount_ExpiredDate_ThrowsValidationException()
        {
            var request = new CreateDiscountRequest
            {
                Scope = "common",
                Type = "percentage",
                Value = 10,
                MinOrderValue = 0,
                UsageLimit = 10,
                ExpiresAt = DateTime.Now.AddDays(-1)
            };

            var ex = Assert.ThrowsAsync<ValidationException>(async () =>
                await _discountService.CreateDiscount(request));
            Assert.That(ex.Message, Contains.Substring("Expiry date must be in future"));
        }

        [Test]
        public void CreateDiscount_CategoryScope_MissingCategoryId_ThrowsValidationException()
        {
            _mockCurrentUserService.Setup(u => u.Role).Returns("Admin");
            var request = new CreateDiscountRequest
            {
                Scope = "category",
                Type = "percentage",
                Value = 10,
                ExpiresAt = DateTime.Now.AddDays(1),
                CategoryId = null
            };
            _mockMapper.Setup(m => m.Map<Discount>(request)).Returns(new Discount());

            var ex = Assert.ThrowsAsync<ValidationException>(async () =>
                await _discountService.CreateDiscount(request));
            Assert.That(ex.Message, Contains.Substring("Category ID is required"));
        }

        [Test]
        public void CreateDiscount_CategoryScope_CategoryNotFound_ThrowsKeyNotFoundException()
        {
            _mockCurrentUserService.Setup(u => u.Role).Returns("Admin");
            _mockCategoryService.Setup(s => s.GetById(10)).ReturnsAsync((CategoryResponse?)null);
            var request = new CreateDiscountRequest
            {
                Scope = "category",
                Type = "percentage",
                Value = 10,
                ExpiresAt = DateTime.Now.AddDays(1),
                CategoryId = 10
            };
            _mockMapper.Setup(m => m.Map<Discount>(request)).Returns(new Discount());

            Assert.ThrowsAsync<KeyNotFoundException>(async () =>
                await _discountService.CreateDiscount(request));
        }

        [Test]
        public void CreateDiscount_ProductScope_MissingProductId_ThrowsValidationException()
        {
            _mockCurrentUserService.Setup(u => u.Role).Returns("Admin");
            var request = new CreateDiscountRequest
            {
                Scope = "product",
                Type = "percentage",
                Value = 10,
                ExpiresAt = DateTime.Now.AddDays(1),
                ProductId = null
            };
            _mockMapper.Setup(m => m.Map<Discount>(request)).Returns(new Discount());

            var ex = Assert.ThrowsAsync<ValidationException>(async () =>
                await _discountService.CreateDiscount(request));
            Assert.That(ex.Message, Contains.Substring("Product ID is required"));
        }

        [Test]
        public void CreateDiscount_ProductScope_ProductNotFound_ThrowsKeyNotFoundException()
        {
            _mockCurrentUserService.Setup(u => u.Role).Returns("Admin");
            _mockProductService.Setup(s => s.GetProductDetails(10)).ReturnsAsync((ProductResponse?)null);
            var request = new CreateDiscountRequest
            {
                Scope = "product",
                Type = "percentage",
                Value = 10,
                ExpiresAt = DateTime.Now.AddDays(1),
                ProductId = 10
            };
            _mockMapper.Setup(m => m.Map<Discount>(request)).Returns(new Discount());

            Assert.ThrowsAsync<KeyNotFoundException>(async () =>
                await _discountService.CreateDiscount(request));
        }

        [Test]
        public void CreateDiscount_VendorScope_NotVendor_ThrowsUnauthorizedException()
        {
            _mockCurrentUserService.Setup(u => u.Role).Returns("Admin");
            var request = new CreateDiscountRequest
            {
                Scope = "vendor",
                Type = "percentage",
                Value = 10,
                ExpiresAt = DateTime.Now.AddDays(1)
            };
            _mockMapper.Setup(m => m.Map<Discount>(request)).Returns(new Discount());

            Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
                await _discountService.CreateDiscount(request));
        }

        [Test]
        public void CreateDiscount_VendorScope_VendorProfileNotFound_ThrowsValidationException()
        {
            _mockCurrentUserService.Setup(u => u.Role).Returns("Vendor");
            _mockCurrentUserService.Setup(u => u.UserId).Returns(1);
            _mockVendorService.Setup(s => s.GetVendorByUserId(1)).ReturnsAsync((VendorProfileResponse?)null);
            
            var request = new CreateDiscountRequest
            {
                Scope = "vendor",
                Type = "percentage",
                Value = 10,
                ExpiresAt = DateTime.Now.AddDays(1)
            };
            _mockMapper.Setup(m => m.Map<Discount>(request)).Returns(new Discount());

            var ex = Assert.ThrowsAsync<ValidationException>(async () =>
                await _discountService.CreateDiscount(request));
            Assert.That(ex.Message, Contains.Substring("Vendor profile not found"));
        }

        [Test]
        public async Task CreateDiscount_ValidCommonScope_CreatesDiscount()
        {
            _mockCurrentUserService.Setup(u => u.Role).Returns("Admin");
            var request = new CreateDiscountRequest
            {
                Scope = "common",
                Type = "percentage",
                Value = 10,
                ExpiresAt = DateTime.Now.AddDays(1)
            };

            var discount = new Discount { Id = 1, Scope = DiscountScope.Common, Value = 10, IsActive = true };
            _mockMapper.Setup(m => m.Map<Discount>(request)).Returns(discount);
            
            _mockDiscountRepo.SetupSequence(r => r.ExistsBycode(It.IsAny<string>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);

            _mockDiscountRepo.Setup(r => r.Create(discount)).ReturnsAsync(discount);
            var expectedResponse = new DiscountResponse { Id = 1, Code = "ABCDEFGH" };
            _mockMapper.Setup(m => m.Map<DiscountResponse>(discount)).Returns(expectedResponse);

            var result = await _discountService.CreateDiscount(request);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Code, Is.EqualTo("ABCDEFGH"));
            _mockDiscountRepo.Verify(r => r.Create(discount), Times.Once);
        }

        [Test]
        public async Task CreateDiscount_ValidCategoryScope_CreatesDiscount()
        {
            _mockCurrentUserService.Setup(u => u.Role).Returns("Admin");
            _mockCategoryService.Setup(s => s.GetById(5)).ReturnsAsync(new CategoryResponse());
            var request = new CreateDiscountRequest
            {
                Scope = "category",
                Type = "percentage",
                Value = 10,
                ExpiresAt = DateTime.Now.AddDays(1),
                CategoryId = 5
            };

            var discount = new Discount { Id = 1, Scope = DiscountScope.Category, Value = 10, CategoryId = 5 };
            _mockMapper.Setup(m => m.Map<Discount>(request)).Returns(discount);
            _mockDiscountRepo.Setup(r => r.ExistsBycode(It.IsAny<string>())).ReturnsAsync(false);
            _mockDiscountRepo.Setup(r => r.Create(discount)).ReturnsAsync(discount);

            await _discountService.CreateDiscount(request);

            _mockDiscountRepo.Verify(r => r.Create(discount), Times.Once);
        }

        [Test]
        public async Task CreateDiscount_ValidProductScope_CreatesDiscount()
        {
            _mockCurrentUserService.Setup(u => u.Role).Returns("Admin");
            _mockProductService.Setup(s => s.GetProductDetails(12)).ReturnsAsync(new ProductResponse());
            var request = new CreateDiscountRequest
            {
                Scope = "product",
                Type = "percentage",
                Value = 10,
                ExpiresAt = DateTime.Now.AddDays(1),
                ProductId = 12
            };

            var discount = new Discount { Id = 1, Scope = DiscountScope.Product, Value = 10, ProductId = 12 };
            _mockMapper.Setup(m => m.Map<Discount>(request)).Returns(discount);
            _mockDiscountRepo.Setup(r => r.ExistsBycode(It.IsAny<string>())).ReturnsAsync(false);
            _mockDiscountRepo.Setup(r => r.Create(discount)).ReturnsAsync(discount);

            await _discountService.CreateDiscount(request);

            _mockDiscountRepo.Verify(r => r.Create(discount), Times.Once);
        }

        [Test]
        public async Task CreateDiscount_ValidVendorScope_CreatesDiscount()
        {
            _mockCurrentUserService.Setup(u => u.Role).Returns("Vendor");
            _mockCurrentUserService.Setup(u => u.UserId).Returns(1);
            _mockVendorService.Setup(s => s.GetVendorByUserId(1)).ReturnsAsync(new VendorProfileResponse { Id = 20 });
            
            var request = new CreateDiscountRequest
            {
                Scope = "vendor",
                Type = "percentage",
                Value = 10,
                ExpiresAt = DateTime.Now.AddDays(1)
            };

            var discount = new Discount { Id = 1, Scope = DiscountScope.Vendor, Value = 10, VendorId = 20 };
            _mockMapper.Setup(m => m.Map<Discount>(request)).Returns(discount);
            _mockDiscountRepo.Setup(r => r.ExistsBycode(It.IsAny<string>())).ReturnsAsync(false);
            _mockDiscountRepo.Setup(r => r.Create(discount)).ReturnsAsync(discount);

            await _discountService.CreateDiscount(request);

            _mockDiscountRepo.Verify(r => r.Create(discount), Times.Once);
        }

        [Test]
        public async Task QueryMethods_ShouldReturnMappedResponses()
        {
            var pageRequest = new PageRequest { PageNumber = 1, PageSize = 10, SearchTerm = "test" };
            var list = new List<Discount> { new Discount { Id = 1 } };
            var expectedResponse = new List<DiscountResponse> { new DiscountResponse { Id = 1 } };

            _mockDiscountRepo.Setup(r => r.GetActiveDiscounts(1, 10, "test")).ReturnsAsync(list);
            _mockDiscountRepo.Setup(r => r.GetDiscountsOfProduct(2, 3, 4)).ReturnsAsync(list);
            _mockDiscountRepo.Setup(r => r.GetDiscountsByVendorId(5)).ReturnsAsync(list);
            _mockDiscountRepo.Setup(r => r.GetDiscountHistory(1, 10, "test")).ReturnsAsync(list);

            _mockMapper.Setup(m => m.Map<ICollection<DiscountResponse>>(It.IsAny<object>())).Returns(expectedResponse);
            _mockMapper.Setup(m => m.Map<List<DiscountResponse>>(It.IsAny<object>())).Returns(expectedResponse);

            var active = await _discountService.GetActiveDiscounts(pageRequest);
            Assert.That(active.First().Id, Is.EqualTo(1));

            var product = await _discountService.GetDiscountsOfProduct(2, 3, 4);
            Assert.That(product.First().Id, Is.EqualTo(1));

            var vendor = await _discountService.GetVendorDiscounts(5);
            Assert.That(vendor.First().Id, Is.EqualTo(1));

            var all = await _discountService.GetAllDiscounts(pageRequest);
            Assert.That(all.First().Id, Is.EqualTo(1));
        }

        [Test]
        public void DeactivateDiscount_DiscountNotFound_ThrowsKeyNotFoundException()
        {
            _mockDiscountRepo.Setup(r => r.GetByCode("INVALID")).ReturnsAsync((Discount?)null);

            Assert.ThrowsAsync<KeyNotFoundException>(async () => await _discountService.DeactivateDiscount("INVALID"));
        }

        [Test]
        public void DeactivateDiscount_VendorScope_ProfileNotFound_ThrowsValidationException()
        {
            var discount = new Discount { Scope = DiscountScope.Vendor, VendorId = 1 };
            _mockDiscountRepo.Setup(r => r.GetByCode("CODE")).ReturnsAsync(discount);
            _mockCurrentUserService.Setup(u => u.UserId).Returns(1);
            _mockVendorService.Setup(s => s.GetVendorByUserId(1)).ReturnsAsync((VendorProfileResponse?)null);

            Assert.ThrowsAsync<ValidationException>(async () => await _discountService.DeactivateDiscount("CODE"));
        }

        [Test]
        public void DeactivateDiscount_VendorScope_MismatchedVendor_ThrowsUnauthorizedException()
        {
            var discount = new Discount { Scope = DiscountScope.Vendor, VendorId = 2 };
            _mockDiscountRepo.Setup(r => r.GetByCode("CODE")).ReturnsAsync(discount);
            _mockCurrentUserService.Setup(u => u.UserId).Returns(1);
            _mockVendorService.Setup(s => s.GetVendorByUserId(1)).ReturnsAsync(new VendorProfileResponse { Id = 1 });

            Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await _discountService.DeactivateDiscount("CODE"));
        }

        [Test]
        public async Task DeactivateDiscount_VendorScope_ValidDeactivation()
        {
            var discount = new Discount { Id = 1, Scope = DiscountScope.Vendor, VendorId = 2, IsActive = true };
            _mockDiscountRepo.Setup(r => r.GetByCode("CODE")).ReturnsAsync(discount);
            _mockCurrentUserService.Setup(u => u.UserId).Returns(1);
            _mockVendorService.Setup(s => s.GetVendorByUserId(1)).ReturnsAsync(new VendorProfileResponse { Id = 2 });
            _mockDiscountRepo.Setup(r => r.Update(1, discount)).ReturnsAsync(discount);

            var expected = new ToggleDiscountStatusResponse { Id = 1, IsActive = false };
            _mockMapper.Setup(m => m.Map<ToggleDiscountStatusResponse>(discount)).Returns(expected);

            var result = await _discountService.DeactivateDiscount("CODE");

            Assert.That(result, Is.Not.Null);
            Assert.That(result.IsActive, Is.False);
            _mockDiscountRepo.Verify(r => r.Update(1, discount), Times.Once);
        }

        [Test]
        public async Task EvaluateCartDiscounts_ShouldReturnMappedList()
        {
            var request = new CartEvaluationRequest
            {
                SubTotal = 100,
                Items = new List<CartItemEvaluationResponse>
                {
                    new CartItemEvaluationResponse { ProductId = 1, CategoryId = 2, VendorId = 3 }
                }
            };
            var discounts = new List<Discount> { new Discount { Id = 1 } };
            _mockDiscountRepo.Setup(r => r.GetApplicableDiscountsAtCart(new List<int>{1}, new List<int>{2}, new List<int>{3}, 100)).ReturnsAsync(discounts);

            var expected = new List<DiscountCartResponse> { new DiscountCartResponse { Code = "CODE" } };
            _mockMapper.Setup(m => m.Map<ICollection<DiscountCartResponse>>(discounts)).Returns(expected);

            var result = await _discountService.EvaluateCartDiscounts(request);

            Assert.That(result.First().Code, Is.EqualTo("CODE"));
        }

        [Test]
        public void ValidateDiscount_DiscountNotFound_ThrowsValidationException()
        {
            _mockDiscountRepo.Setup(r => r.GetByCode("CODE")).ReturnsAsync((Discount?)null);

            Assert.ThrowsAsync<ValidationException>(async () => await _discountService.ValidateDiscount("CODE", new List<CartItem>(), 100));
        }

        [Test]
        public void ValidateDiscount_DiscountExpired_ThrowsValidationException()
        {
            var discount = new Discount { Code = "CODE", IsActive = true, ExpiresAt = DateTime.Now.AddDays(-1) };
            _mockDiscountRepo.Setup(r => r.GetByCode("CODE")).ReturnsAsync(discount);

            Assert.ThrowsAsync<ValidationException>(async () => await _discountService.ValidateDiscount("CODE", new List<CartItem>(), 100));
        }

        [Test]
        public void ValidateDiscount_ProductScope_ProductMismatched_ThrowsValidationException()
        {
            var discount = new Discount { Scope = DiscountScope.Product, ProductId = 5, IsActive = true, ExpiresAt = DateTime.Now.AddDays(1) };
            _mockDiscountRepo.Setup(r => r.GetByCode("CODE")).ReturnsAsync(discount);
            
            var items = new List<CartItem>
            {
                new CartItem { Variant = new ProductVariant { Product = new Product { Id = 6 } } }
            };

            Assert.ThrowsAsync<ValidationException>(async () => await _discountService.ValidateDiscount("CODE", items, 100));
        }

        [Test]
        public void ValidateDiscount_CategoryScope_CategoryMismatched_ThrowsValidationException()
        {
            var discount = new Discount { Scope = DiscountScope.Category, Category = new Category { Id = 5 }, IsActive = true, ExpiresAt = DateTime.Now.AddDays(1) };
            _mockDiscountRepo.Setup(r => r.GetByCode("CODE")).ReturnsAsync(discount);
            
            var items = new List<CartItem>
            {
                new CartItem { Variant = new ProductVariant { Product = new Product { Category = new Category { Id = 6 } } } }
            };

            Assert.ThrowsAsync<ValidationException>(async () => await _discountService.ValidateDiscount("CODE", items, 100));
        }

        [Test]
        public void ValidateDiscount_VendorScope_VendorMismatched_ThrowsValidationException()
        {
            var discount = new Discount { Scope = DiscountScope.Vendor, Vendor = new Vendor { Id = 5 }, IsActive = true, ExpiresAt = DateTime.Now.AddDays(1) };
            _mockDiscountRepo.Setup(r => r.GetByCode("CODE")).ReturnsAsync(discount);
            
            var items = new List<CartItem>
            {
                new CartItem { Variant = new ProductVariant { Product = new Product { Vendor = new Vendor { Id = 6 } } } }
            };

            Assert.ThrowsAsync<ValidationException>(async () => await _discountService.ValidateDiscount("CODE", items, 100));
        }

        [Test]
        public void ValidateDiscount_SubtotalLessThanMinOrderValue_ThrowsValidationException()
        {
            var discount = new Discount { Scope = DiscountScope.Common, MinOrderValue = 150, IsActive = true, ExpiresAt = DateTime.Now.AddDays(1) };
            _mockDiscountRepo.Setup(r => r.GetByCode("CODE")).ReturnsAsync(discount);

            Assert.ThrowsAsync<ValidationException>(async () => await _discountService.ValidateDiscount("CODE", new List<CartItem>(), 100));
        }

        [Test]
        public async Task ValidateDiscount_Valid_ReturnsDiscount()
        {
            var discount = new Discount { Scope = DiscountScope.Common, MinOrderValue = 50, IsActive = true, ExpiresAt = DateTime.Now.AddDays(1) };
            _mockDiscountRepo.Setup(r => r.GetByCode("CODE")).ReturnsAsync(discount);

            var result = await _discountService.ValidateDiscount("CODE", new List<CartItem>(), 100);

            Assert.That(result, Is.EqualTo(discount));
        }

        [Test]
        public async Task CalculateDiscountAmount_PercentageAndFlat_CalculatesCorrectly()
        {
            var flat = new Discount { Type = DiscountType.Flat, Value = 15 };
            var percentage = new Discount { Type = DiscountType.Percentage, Value = 10 };

            var flatAmt = await _discountService.CalculateDiscountAmount(flat, 100);
            Assert.That(flatAmt, Is.EqualTo(15));

            var pctAmt = await _discountService.CalculateDiscountAmount(percentage, 125.55m);
            Assert.That(pctAmt, Is.EqualTo(12.56m));
        }

        [Test]
        public void CreateDiscount_InvalidEnumScopeButParsesAsInt_ThrowsValidationException()
        {
            var request = new CreateDiscountRequest
            {
                Scope = "100", 
                Type = "percentage",
                Value = 10,
                ExpiresAt = DateTime.Now.AddDays(1)
            };
            _mockMapper.Setup(m => m.Map<Discount>(request)).Returns(new Discount());

            var ex = Assert.ThrowsAsync<ValidationException>(async () =>
                await _discountService.CreateDiscount(request));
            Assert.That(ex.Message, Contains.Substring("Unsupported discount scope"));
        }

        [Test]
        public void CreateDiscount_CommonScope_NotAdmin_ThrowsUnauthorizedAccessException()
        {
            _mockCurrentUserService.Setup(u => u.Role).Returns("Vendor");
            var request = new CreateDiscountRequest
            {
                Scope = "common",
                Type = "percentage",
                Value = 10,
                ExpiresAt = DateTime.Now.AddDays(1)
            };
            _mockMapper.Setup(m => m.Map<Discount>(request)).Returns(new Discount());

            Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
                await _discountService.CreateDiscount(request));
        }

        [Test]
        public void DeactivateDiscount_CommonScope_NotAdmin_ThrowsUnauthorizedAccessException()
        {
            var discount = new Discount { Scope = DiscountScope.Common };
            _mockDiscountRepo.Setup(r => r.GetByCode("CODE")).ReturnsAsync(discount);
            _mockCurrentUserService.Setup(u => u.Role).Returns("Vendor");

            Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await _discountService.DeactivateDiscount("CODE"));
        }
    }
}

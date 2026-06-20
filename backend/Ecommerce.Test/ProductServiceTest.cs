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
using Ecommerce.Shared.Exceptions;
using Moq;
using NUnit.Framework;

namespace Ecommerce.Test
{
    [TestFixture]
    public class ProductServiceTest
    {
        private Mock<IProductRepository> _mockProductRepo;
        private Mock<IVendorRepository> _mockVendorRepo;
        private Mock<ICurrentUserService> _mockCurrentUser;
        private Mock<IMapper> _mockMapper;
        private ProductService _productService;

        [SetUp]
        public void Setup()
        {
            _mockProductRepo = new Mock<IProductRepository>();
            _mockVendorRepo = new Mock<IVendorRepository>();
            _mockCurrentUser = new Mock<ICurrentUserService>();
            _mockMapper = new Mock<IMapper>();

            _productService = new ProductService(
                _mockProductRepo.Object,
                _mockVendorRepo.Object,
                _mockCurrentUser.Object,
                _mockMapper.Object
            );
        }

        [Test]
        public void GetProductsCatalog_ShouldThrowValidationException_WhenSortOrderIsInvalid()
        {
           
            var request = new ProductFilterRequest { SortOrder = "invalid", SortBy = "price" };

            Assert.ThrowsAsync<ValidationException>(async () => await _productService.GetProductsCatalog(request));
        }

        [Test]
        public void GetProductsCatalog_ShouldThrowValidationException_WhenSortByIsInvalid()
        {
           
            var request = new ProductFilterRequest { SortOrder = "asc", SortBy = "invalid" };

            Assert.ThrowsAsync<ValidationException>(async () => await _productService.GetProductsCatalog(request));
        }

        [Test]
        public async Task GetProductsCatalog_ShouldReturnPageResponse_WhenRequestIsValid()
        {
           
            var request = new ProductFilterRequest { PageNumber = 1, PageSize = 10, SortBy = "price", SortOrder = "asc", CategoryId = 5 };
            var products = new List<Product> { new Product { Id = 1 } };
            _mockProductRepo.Setup(r => r.GetProductsPaged(1, 10, "price", "asc", 5)).ReturnsAsync(products);
            _mockProductRepo.Setup(r => r.GetProductsCount(5)).ReturnsAsync(1);

            var mappedList = new List<ProductResponse> { new ProductResponse { Id = 1 } };
            _mockMapper.Setup(m => m.Map<ICollection<ProductResponse>>(products)).Returns(mappedList);

            
            var result = await _productService.GetProductsCatalog(request);

            
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Items.First().Id, Is.EqualTo(1));
            Assert.That(result.TotalCount, Is.EqualTo(1));
        }

        [Test]
        public async Task SearchProducts_ShouldReturnMappedList()
        {
           
            var products = new List<Product> { new Product { Id = 1 } };
            _mockProductRepo.Setup(r => r.SearchProductsByName("test")).ReturnsAsync(products);

            var mappedList = new List<ProductResponse> { new ProductResponse { Id = 1 } };
            _mockMapper.Setup(m => m.Map<ICollection<ProductResponse>>(products)).Returns(mappedList);

            
            var result = await _productService.SearchProducts("test");

            
            Assert.That(result.First().Id, Is.EqualTo(1));
        }

        [Test]
        public void GetProductDetails_ShouldThrowException_WhenProductNotFound()
        {
           
            _mockProductRepo.Setup(r => r.GetById(1)).ReturnsAsync((Product?)null);

            Assert.ThrowsAsync<InvalidProductException>(async () => await _productService.GetProductDetails(1));
        }

        [Test]
        public async Task GetProductDetails_ShouldReturnResponse_WhenProductFound()
        {
           
            var product = new Product { Id = 1 };
            _mockProductRepo.Setup(r => r.GetById(1)).ReturnsAsync(product);

            var expected = new ProductResponse { Id = 1 };
            _mockMapper.Setup(m => m.Map<ProductResponse>(product)).Returns(expected);

            
            var result = await _productService.GetProductDetails(1);

            
            Assert.That(result.Id, Is.EqualTo(1));
        }

        [Test]
        public void GetVendorProducts_ShouldThrowException_WhenVendorNotFound()
        {
           
            _mockCurrentUser.Setup(u => u.UserId).Returns(1);
            _mockVendorRepo.Setup(r => r.GetByUserId(1)).ReturnsAsync((Vendor?)null);

            Assert.ThrowsAsync<InvalidOwnershipException>(async () => await _productService.GetVendorProducts());
        }

        [Test]
        public async Task GetVendorProducts_ShouldReturnMappedProducts()
        {
           
            _mockCurrentUser.Setup(u => u.UserId).Returns(1);
            var vendor = new Vendor { Id = 10 };
            _mockVendorRepo.Setup(r => r.GetByUserId(1)).ReturnsAsync(vendor);

            var products = new List<Product> { new Product { Id = 1 } };
            _mockProductRepo.Setup(r => r.GetProductsByVendorId(10)).ReturnsAsync(products);

            var expected = new List<ProductResponse> { new ProductResponse { Id = 1 } };
            _mockMapper.Setup(m => m.Map<ICollection<ProductResponse>>(products)).Returns(expected);

            
            var result = await _productService.GetVendorProducts();

            
            Assert.That(result.First().Id, Is.EqualTo(1));
        }

        [Test]
        public async Task CreateProduct_ShouldCreateProduct_WhenValid()
        {
           
            _mockCurrentUser.Setup(u => u.UserId).Returns(1);
            var vendor = new Vendor { Id = 10 };
            _mockVendorRepo.Setup(r => r.GetByUserId(1)).ReturnsAsync(vendor);

            var request = new CreateProductRequest { Name = "New Product" };
            var product = new Product { Name = "New Product" };
            _mockMapper.Setup(m => m.Map<Product>(request)).Returns(product);

            var created = new Product { Id = 100, Name = "New Product" };
            _mockProductRepo.Setup(r => r.Create(product)).ReturnsAsync(created);

            var expected = new ProductResponse { Id = 100, Name = "New Product" };
            _mockMapper.Setup(m => m.Map<ProductResponse>(created)).Returns(expected);

            
            var result = await _productService.CreateProduct(request);

            
            Assert.That(result.Id, Is.EqualTo(100));
            Assert.That(product.VendorId, Is.EqualTo(10));
            Assert.That(product.Status, Is.EqualTo(ProductStatus.Draft));
        }

        [Test]
        public void UpdateProduct_ShouldThrowException_WhenProductNotFound()
        {
           
            _mockProductRepo.Setup(r => r.GetById(100)).ReturnsAsync((Product?)null);
            var request = new UpdateProductRequest { Name = "Updated" };

            Assert.ThrowsAsync<InvalidProductException>(async () => await _productService.UpdateProduct(100, request));
        }

        [Test]
        public void UpdateProduct_ShouldThrowException_WhenNotOwnedByVendor()
        {
           
            var product = new Product { Id = 100, VendorId = 2 };
            _mockProductRepo.Setup(r => r.GetById(100)).ReturnsAsync(product);
            
            _mockCurrentUser.Setup(u => u.UserId).Returns(1);
            _mockVendorRepo.Setup(r => r.GetByUserId(1)).ReturnsAsync(new Vendor { Id = 3 });

            var request = new UpdateProductRequest { Name = "Updated" };

            Assert.ThrowsAsync<InvalidOwnershipException>(async () => await _productService.UpdateProduct(100, request));
        }

        [Test]
        public async Task UpdateProduct_ShouldUpdateFields_WhenValid()
        {
           
            var product = new Product { Id = 100, VendorId = 10, Name = "Old", Description = "Old", CategoryId = 1 };
            _mockProductRepo.Setup(r => r.GetById(100)).ReturnsAsync(product);

            _mockCurrentUser.Setup(u => u.UserId).Returns(1);
            _mockVendorRepo.Setup(r => r.GetByUserId(1)).ReturnsAsync(new Vendor { Id = 10 });

            _mockProductRepo.Setup(r => r.Update(100, product)).ReturnsAsync(product);
            var expected = new ProductResponse { Id = 100, Name = "New" };
            _mockMapper.Setup(m => m.Map<ProductResponse>(product)).Returns(expected);

            var request = new UpdateProductRequest { Name = "New", Description = "New", CategoryId = 2 };

            
            var result = await _productService.UpdateProduct(100, request);

            
            Assert.That(result.Name, Is.EqualTo("New"));
            Assert.That(product.Name, Is.EqualTo("New"));
            Assert.That(product.Description, Is.EqualTo("New"));
            Assert.That(product.CategoryId, Is.EqualTo(2));
        }

        [Test]
        public void ArchiveProduct_ShouldThrowException_WhenProductNotFound()
        {
           
            _mockProductRepo.Setup(r => r.GetById(100)).ReturnsAsync((Product?)null);

            Assert.ThrowsAsync<InvalidProductException>(async () => await _productService.ArchiveProduct(100));
        }

        [Test]
        public async Task ArchiveProduct_ShouldArchiveProductAndVariants_WhenValid()
        {
           
            var variant = new ProductVariant { Id = 5, IsActive = true };
            var product = new Product { Id = 100, VendorId = 10, Status = ProductStatus.Draft, Variants = new List<ProductVariant> { variant } };
            _mockProductRepo.Setup(r => r.GetById(100)).ReturnsAsync(product);

            _mockCurrentUser.Setup(u => u.UserId).Returns(1);
            _mockVendorRepo.Setup(r => r.GetByUserId(1)).ReturnsAsync(new Vendor { Id = 10 });

            _mockProductRepo.Setup(r => r.Update(100, product)).ReturnsAsync(product);
            _mockMapper.Setup(m => m.Map<ProductResponse>(product)).Returns(new ProductResponse { Id = 100 });

            
            await _productService.ArchiveProduct(100);

            
            Assert.That(product.Status, Is.EqualTo(ProductStatus.Archived));
            Assert.That(variant.IsActive, Is.False);
            _mockProductRepo.Verify(r => r.Update(100, product), Times.Once);
        }

        [Test]
        public void PublishProduct_ShouldThrowException_WhenProductNotFound()
        {
           
            _mockProductRepo.Setup(r => r.GetById(100)).ReturnsAsync((Product?)null);

            Assert.ThrowsAsync<InvalidProductException>(async () => await _productService.PublishProduct(100));
        }

        [Test]
        public void PublishProduct_ShouldThrowException_WhenVendorNotFound()
        {
           
            _mockProductRepo.Setup(r => r.GetById(100)).ReturnsAsync(new Product());
            _mockCurrentUser.Setup(u => u.UserId).Returns(1);
            _mockVendorRepo.Setup(r => r.GetByUserId(1)).ReturnsAsync((Vendor?)null);

            Assert.ThrowsAsync<InvalidOwnershipException>(async () => await _productService.PublishProduct(100));
        }

        [Test]
        public void PublishProduct_ShouldThrowException_WhenNotOwnProduct()
        {
           
            var product = new Product { VendorId = 2 };
            _mockProductRepo.Setup(r => r.GetById(100)).ReturnsAsync(product);
            _mockCurrentUser.Setup(u => u.UserId).Returns(1);
            _mockVendorRepo.Setup(r => r.GetByUserId(1)).ReturnsAsync(new Vendor { Id = 3 });

            Assert.ThrowsAsync<InvalidOwnershipException>(async () => await _productService.PublishProduct(100));
        }

        [Test]
        public void PublishProduct_ShouldThrowException_WhenNoValidVariant()
        {
           
            var variant = new ProductVariant { IsActive = false, Price = 10.00m, StockQty = 5 };
            var product = new Product { VendorId = 10, Variants = new List<ProductVariant> { variant } };
            _mockProductRepo.Setup(r => r.GetById(100)).ReturnsAsync(product);
            _mockCurrentUser.Setup(u => u.UserId).Returns(1);
            _mockVendorRepo.Setup(r => r.GetByUserId(1)).ReturnsAsync(new Vendor { Id = 10 });

            Assert.ThrowsAsync<InvalidOperationException>(async () => await _productService.PublishProduct(100));
        }

        [Test]
        public async Task PublishProduct_ShouldPublishProduct_WhenValid()
        {
           
            var variant = new ProductVariant { IsActive = true, Price = 10.00m, StockQty = 5 };
            var product = new Product { Id = 100, VendorId = 10, Status = ProductStatus.Draft, Variants = new List<ProductVariant> { variant } };
            _mockProductRepo.Setup(r => r.GetById(100)).ReturnsAsync(product);
            _mockCurrentUser.Setup(u => u.UserId).Returns(1);
            _mockVendorRepo.Setup(r => r.GetByUserId(1)).ReturnsAsync(new Vendor { Id = 10 });

            _mockProductRepo.Setup(r => r.Update(100, product)).ReturnsAsync(product);
            _mockMapper.Setup(m => m.Map<ProductResponse>(product)).Returns(new ProductResponse { Id = 100 });

            
            var result = await _productService.PublishProduct(100);

            
            Assert.That(product.Status, Is.EqualTo(ProductStatus.Active));
            _mockProductRepo.Verify(r => r.Update(100, product), Times.Once);
        }
    }
}

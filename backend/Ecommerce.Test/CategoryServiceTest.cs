using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Ecommerce.BLL;
using Ecommerce.Contracts.Repositories;
using Ecommerce.Models;
using Ecommerce.Models.DTOs;
using Ecommerce.Shared.Exceptions;
using Moq;
using NUnit.Framework;

namespace Ecommerce.Test
{
    [TestFixture]
    public class CategoryServiceTest
    {
        private Mock<ICategoryRepository> _mockCategoryRepo;
        private Mock<IMapper> _mockMapper;
        private CategoryService _categoryService;

        [SetUp]
        public void Setup()
        {
            _mockCategoryRepo = new Mock<ICategoryRepository>();
            _mockMapper = new Mock<IMapper>();
            _categoryService = new CategoryService(_mockCategoryRepo.Object, _mockMapper.Object);
        }

        [Test]
        public async Task GetById_PassTest_ReturnsCategoryResponse()
        {
            var category = new Category { Id = 1, Name = "electronics", slug = "electronics", ParentId = null };
            _mockCategoryRepo.Setup(r => r.GetById(1)).ReturnsAsync(category);
            _mockCategoryRepo.Setup(r => r.GetProductCount(1)).ReturnsAsync(5);

            var expectedResponse = new CategoryResponse { Id = 1, Name = "electronics", Slug = "electronics", ParentId = null, ProductCount = 5 };
            _mockMapper.Setup(m => m.Map<CategoryResponse>(category)).Returns(expectedResponse);

            var result = await _categoryService.GetById(1);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(1));
            Assert.That(result.Name, Is.EqualTo("electronics"));
            Assert.That(result.ProductCount, Is.EqualTo(5));
        }

        [Test]
        public void GetById_CategoryNotFound_ThrowsKeyNotFoundException()
        {
            _mockCategoryRepo.Setup(r => r.GetById(99)).ReturnsAsync((Category?)null);

            Assert.ThrowsAsync<KeyNotFoundException>(async () => await _categoryService.GetById(99));
        }

        [Test]
        public void Create_ShouldThrowException_WhenSlugExists()
        {
            _mockCategoryRepo.Setup(r => r.SlugExists("electronics")).ReturnsAsync(true);
            var request = new CreateCategoryRequest { Name = "Electronics", Slug = "electronics" };

            Assert.ThrowsAsync<UniquenessViolationException>(async () => await _categoryService.Create(request));
        }

        [Test]
        public void Create_ShouldThrowException_WhenNameExists()
        {
            _mockCategoryRepo.Setup(r => r.SlugExists("electronics")).ReturnsAsync(false);
            _mockCategoryRepo.Setup(r => r.NameExists("electronics")).ReturnsAsync(true);
            var request = new CreateCategoryRequest { Name = "electronics", Slug = "electronics" };

            Assert.ThrowsAsync<UniquenessViolationException>(async () => await _categoryService.Create(request));
        }

        [Test]
        public async Task Create_ShouldCreateCategory_WhenRequestIsValid()
        {
            _mockCategoryRepo.Setup(r => r.SlugExists("electronics")).ReturnsAsync(false);
            _mockCategoryRepo.Setup(r => r.NameExists("electronics")).ReturnsAsync(false);
            
            var createdCategory = new Category { Id = 1, Name = "electronics", slug = "electronics" };
            _mockCategoryRepo.Setup(r => r.Create(It.IsAny<Category>())).ReturnsAsync(createdCategory);
            _mockCategoryRepo.Setup(r => r.GetProductCount(1)).ReturnsAsync(0);

            var expectedResponse = new CategoryResponse { Id = 1, Name = "electronics", Slug = "electronics", ProductCount = 0 };
            _mockMapper.Setup(m => m.Map<CategoryResponse>(createdCategory)).Returns(expectedResponse);

            var request = new CreateCategoryRequest { Name = "electronics", Slug = "electronics" };

            var result = await _categoryService.Create(request);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(1));
            _mockCategoryRepo.Verify(r => r.Create(It.IsAny<Category>()), Times.Once);
        }

        [Test]
        public void Update_ShouldThrowException_WhenCategoryNotFound()
        {
            _mockCategoryRepo.Setup(r => r.GetById(1)).ReturnsAsync((Category?)null);
            var request = new UpdateCategoryRequest { Name = "Electronics", Slug = "electronics" };

            Assert.ThrowsAsync<KeyNotFoundException>(async () => await _categoryService.Update(1, request));
        }

        [Test]
        public void Update_ShouldThrowException_WhenSettingCategoryAsItsOwnParent()
        {
            var existing = new Category { Id = 1, Name = "electronics", slug = "electronics" };
            _mockCategoryRepo.Setup(r => r.GetById(1)).ReturnsAsync(existing);
            var request = new UpdateCategoryRequest { ParentId = 1 };

            Assert.ThrowsAsync<InvalidAncestorException>(async () => await _categoryService.Update(1, request));
        }

        [Test]
        public void Update_ShouldThrowException_WhenSettingDescendantAsParent()
        {
            var existing = new Category { Id = 1, Name = "electronics", slug = "electronics" };
            _mockCategoryRepo.Setup(r => r.GetById(1)).ReturnsAsync(existing);
            _mockCategoryRepo.Setup(r => r.GetAncestorIds(2)).ReturnsAsync(new List<int> { 1, 3 });
            var request = new UpdateCategoryRequest { ParentId = 2 };

            Assert.ThrowsAsync<InvalidAncestorException>(async () => await _categoryService.Update(1, request));
        }

        [Test]
        public void Update_ShouldThrowException_WhenSlugAlreadyInUse()
        {
            var existing = new Category { Id = 1, Name = "electronics", slug = "electronics" };
            _mockCategoryRepo.Setup(r => r.GetById(1)).ReturnsAsync(existing);
            _mockCategoryRepo.Setup(r => r.SlugExists("new-slug")).ReturnsAsync(true);
            var request = new UpdateCategoryRequest { Slug = "new-slug" };

            Assert.ThrowsAsync<UniquenessViolationException>(async () => await _categoryService.Update(1, request));
        }

        [Test]
        public void Update_ShouldThrowException_WhenNameAlreadyInUse()
        {
            var existing = new Category { Id = 1, Name = "electronics", slug = "electronics" };
            _mockCategoryRepo.Setup(r => r.GetById(1)).ReturnsAsync(existing);
            _mockCategoryRepo.Setup(r => r.NameExists("new-name")).ReturnsAsync(true);
            var request = new UpdateCategoryRequest { Name = "new-name" };

            Assert.ThrowsAsync<UniquenessViolationException>(async () => await _categoryService.Update(1, request));
        }

        [Test]
        public async Task Update_ShouldUpdateFields_WhenRequestIsValid()
        {
            var existing = new Category { Id = 1, Name = "electronics", slug = "electronics", ParentId = null };
            _mockCategoryRepo.Setup(r => r.GetById(1)).ReturnsAsync(existing);
            _mockCategoryRepo.Setup(r => r.GetAncestorIds(2)).ReturnsAsync(new List<int> { 3 });
            _mockCategoryRepo.Setup(r => r.SlugExists("new-slug")).ReturnsAsync(false);
            _mockCategoryRepo.Setup(r => r.NameExists("new-name")).ReturnsAsync(false);

            var updated = new Category { Id = 1, Name = "new-name", slug = "new-slug", ParentId = 2 };
            _mockCategoryRepo.Setup(r => r.Update(1, existing)).ReturnsAsync(updated);
            _mockCategoryRepo.Setup(r => r.GetProductCount(1)).ReturnsAsync(10);

            var expectedResponse = new CategoryResponse { Id = 1, Name = "new-name", Slug = "new-slug", ParentId = 2, ProductCount = 10 };
            _mockMapper.Setup(m => m.Map<CategoryResponse>(updated)).Returns(expectedResponse);

            var request = new UpdateCategoryRequest { Name = "new-name", Slug = "new-slug", ParentId = 2 };

            var result = await _categoryService.Update(1, request);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Name, Is.EqualTo("new-name"));
            Assert.That(result.Slug, Is.EqualTo("new-slug"));
            Assert.That(result.ParentId, Is.EqualTo(2));
            _mockCategoryRepo.Verify(r => r.Update(1, existing), Times.Once);
        }

        [Test]
        public async Task GetCategoryTree_ShouldReturnTree()
        {
            var root = new Category { Id = 1, Name = "electronics", slug = "electronics", ParentId = null, Products = new List<Product>() };
            var child = new Category { Id = 2, Name = "phones", slug = "phones", ParentId = 1, Products = new List<Product>() };
            var categories = new List<Category> { root, child };

            _mockCategoryRepo.Setup(r => r.GetAllWithChildren()).ReturnsAsync(categories);

            var rootTree = new CategoryTreeResponse { Id = 1, Name = "electronics", Slug = "electronics" };
            var childTree = new CategoryTreeResponse { Id = 2, Name = "phones", Slug = "phones" };

            _mockMapper.Setup(m => m.Map<CategoryTreeResponse>(root)).Returns(rootTree);
            _mockMapper.Setup(m => m.Map<CategoryTreeResponse>(child)).Returns(childTree);

            var result = await _categoryService.GetCategoryTree();

            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result.First().Id, Is.EqualTo(1));
        }

        [Test]
        public void GetBySlug_ShouldThrowKeyNotFoundException_WhenCategoryNotFound()
        {
            _mockCategoryRepo.Setup(r => r.GetBySlug("invalid")).ReturnsAsync((Category?)null);

            Assert.ThrowsAsync<KeyNotFoundException>(async () => await _categoryService.GetBySlug("invalid"));
        }

        [Test]
        public async Task GetBySlug_ShouldReturnTreeResponse_WhenFound()
        {
            var category = new Category { Id = 1, Name = "electronics", slug = "electronics" };
            _mockCategoryRepo.Setup(r => r.GetBySlug("electronics")).ReturnsAsync(category);

            var expected = new CategoryTreeResponse { Id = 1, Name = "electronics" };
            _mockMapper.Setup(m => m.Map<CategoryTreeResponse>(category)).Returns(expected);

            var result = await _categoryService.GetBySlug("electronics");

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(1));
        }

        [Test]
        public void Delete_ShouldThrowKeyNotFoundException_WhenCategoryNotFound()
        {
            _mockCategoryRepo.Setup(r => r.GetById(1)).ReturnsAsync((Category?)null);

            Assert.ThrowsAsync<KeyNotFoundException>(async () => await _categoryService.Delete(1));
        }

        [Test]
        public void Delete_ShouldThrowInvalidOperationException_WhenCategoryHasProducts()
        {
            var category = new Category { Id = 1 };
            _mockCategoryRepo.Setup(r => r.GetById(1)).ReturnsAsync(category);
            _mockCategoryRepo.Setup(r => r.HasProducts(1)).ReturnsAsync(true);

            Assert.ThrowsAsync<InvalidOperationException>(async () => await _categoryService.Delete(1));
        }

        [Test]
        public async Task Delete_ShouldDelete_WhenRequestIsValid()
        {
            var category = new Category { Id = 1 };
            _mockCategoryRepo.Setup(r => r.GetById(1)).ReturnsAsync(category);
            _mockCategoryRepo.Setup(r => r.HasProducts(1)).ReturnsAsync(false);
            _mockCategoryRepo.Setup(r => r.Delete(1)).ReturnsAsync(category);

            var expected = new CategoryStatusResponse { Id = 1 };
            _mockMapper.Setup(m => m.Map<CategoryStatusResponse>(category)).Returns(expected);

            var result = await _categoryService.Delete(1);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(1));
            _mockCategoryRepo.Verify(r => r.Delete(1), Times.Once);
        }

        [Test]
        public async Task GetFlatList_ShouldReturnFlatList()
        {
            var categories = new List<Category> { new Category { Id = 1 } };
            _mockCategoryRepo.Setup(r => r.GetAll()).ReturnsAsync(categories);
            _mockCategoryRepo.Setup(r => r.GetProductCount(1)).ReturnsAsync(2);

            var mappedResponse = new CategoryResponse { Id = 1 };
            _mockMapper.Setup(m => m.Map<CategoryResponse>(categories.First())).Returns(mappedResponse);

            var result = await _categoryService.GetFlatList();

            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result.First().ProductCount, Is.EqualTo(2));
        }
    }
}

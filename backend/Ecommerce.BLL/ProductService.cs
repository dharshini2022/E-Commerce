using AutoMapper;
using Ecommerce.Contracts.Repositories;
using Ecommerce.Contracts.Services;
using Ecommerce.Models;
using Ecommerce.Models.DTOs;
using Ecommerce.Shared.Exceptions;

namespace Ecommerce.BLL
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IVendorRepository _vendorRepository;
        private readonly ICurrentUserService _currentUser;
        private readonly IMapper _mapper;

        public ProductService(IProductRepository productRepository, IVendorRepository vendorRepository, ICurrentUserService currentUser, IMapper mapper)
        {
            _productRepository = productRepository;
            _vendorRepository = vendorRepository;
            _currentUser = currentUser;
            _mapper = mapper;
        }

        public async Task<PageResponse<ProductResponse>> GetProductsCatalog(ProductFilterRequest query)
        {
            if (!string.IsNullOrEmpty(query.SortOrder) && !(query.SortOrder == "asc" || query.SortOrder == "desc"))
                throw new ValidationException("Sort Order must be 'asc' or 'desc'");
            if (!string.IsNullOrEmpty(query.SortBy) && !(query.SortBy == "price" || query.SortBy == "newest" || query.SortBy == "review" || query.SortBy == "rating"))
                throw new ValidationException("Sort By must be 'price', 'newest', 'review', or 'rating'");

            var products = await _productRepository.GetProductsPaged(query.PageNumber, query.PageSize, query.SortBy, query.SortOrder, query.CategoryId);
            var totalCount = await _productRepository.GetProductsCount(query.CategoryId);

            return new PageResponse<ProductResponse>
            {
                Items = _mapper.Map<ICollection<ProductResponse>>(products),
                PageNumber = query.PageNumber,
                PageSize = query.PageSize,
                TotalCount = totalCount
            };
        }

        public async Task<ICollection<ProductResponse>> SearchProducts(string searchTerm)
        {
            var products = await _productRepository.SearchProductsByName(searchTerm);
            return _mapper.Map<ICollection<ProductResponse>>(products);    
        }

        public async Task<ProductResponse> GetProductDetails(int productId)
        {
            var product = await _productRepository.GetById(productId) ?? throw new InvalidProductException($"Product with ID: {productId} not found.");
            return _mapper.Map<ProductResponse>(product);
        }

        public async Task<ICollection<ProductResponse>> GetVendorProducts()
        {
            var vendor = await _vendorRepository.GetByUserId(_currentUser.UserId) ?? throw new InvalidOwnershipException("Vendor account not found.");
            var products = await _productRepository.GetProductsByVendorId(vendor.Id);
            return _mapper.Map<ICollection<ProductResponse>>(products);
        }

        public async Task<ProductResponse> CreateProduct(CreateProductRequest request)
        {
            var vendorId = await GetCurrentVendorId();

            var product = _mapper.Map<Product>(request);
            product.VendorId = vendorId;
            product.CreatedAt = DateTime.Now;
            product.Status = ProductStatus.Draft;

            var created = await _productRepository.Create(product);
            return _mapper.Map<ProductResponse>(created);
        }

        public async Task<ProductResponse> UpdateProduct(int productId, UpdateProductRequest request)
        {
            var product = await _productRepository.GetById(productId) ?? throw new InvalidProductException($"Product with ID {productId} not found.");

            await EnsureVendorOwns(product);

            if (!string.IsNullOrEmpty(request.Name)) product.Name = request.Name;
            if (!string.IsNullOrEmpty(request.Description)) product.Description = request.Description;
            if (request.CategoryId.HasValue) product.CategoryId = request.CategoryId.Value;

            await _productRepository.Update(productId, product);
            return _mapper.Map<ProductResponse>(product);
        }

        public async Task<ProductResponse> ArchiveProduct(int productId)
        {
            var product = await _productRepository.GetById(productId) ?? throw new InvalidProductException($"Product with ID {productId} not found.");

            await EnsureVendorOwns(product);

            product.Status = ProductStatus.Archived;
            foreach(var variant in product.Variants)
            {
                variant.IsActive = false;
            }
            await _productRepository.Update(productId, product);
            return _mapper.Map<ProductResponse>(product);
        }

        public async Task<ProductResponse?> PublishProduct(int productId)
        {
            var product = await _productRepository.GetById(productId) ?? throw new InvalidProductException("Product not found.");
            var vendor = await _vendorRepository.GetByUserId(_currentUser.UserId) ?? throw new InvalidOwnershipException("Vendor account not found.");

            if (product.VendorId != vendor.Id) throw new InvalidOwnershipException("You don't own this product.");
            bool hasValidVariant = product.Variants.Any(v => v.IsActive && v.Price > 0 && v.StockQty >= 0);
            if (!hasValidVariant) throw new InvalidOperationException("Product must contain at least one valid variant.");

            product.Status = ProductStatus.Active;
            var result = await _productRepository.Update(productId, product);
            return _mapper.Map<ProductResponse>(result);
        }

        private async Task EnsureVendorOwns(Product product)
        {
            var vendorId = await GetCurrentVendorId();
            if (product.VendorId != vendorId) throw new InvalidOwnershipException("Access denied! You do not own this product.");
        }

        private async Task<int> GetCurrentVendorId()
        {
            var vendor = await _vendorRepository.GetByUserId(_currentUser.UserId) ?? throw new InvalidOwnershipException("Vendor account not found.");
            return vendor.Id;
        }
    }
}

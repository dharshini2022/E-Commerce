using AutoMapper;
using Ecommerce.Contracts.Repositories;
using Ecommerce.Contracts.Services;
using Ecommerce.Models;
using Ecommerce.Models.DTOs;
using Ecommerce.Shared.Exceptions;

namespace Ecommerce.BLL
{
    public class ProductVariantService : IProductVariantService
    {
        private readonly IProductVariantRepository _variantRepository;
        private readonly IProductImageRepository _imageRepository;
        private readonly IProductRepository _productRepository;
        private readonly IVendorRepository _vendorRepository;
        private readonly ICurrentUserService _currentUser;
        private readonly IMapper _mapper;
        private readonly IStockReservationRepository _stockReservationRepository;

        public ProductVariantService(IProductVariantRepository variantRepository, IProductImageRepository imageRepository, IProductRepository productRepository, IVendorRepository vendorRepository, ICurrentUserService currentUser,  IMapper mapper, IStockReservationRepository stockReservationRepository)
        {
            _variantRepository = variantRepository;
            _imageRepository = imageRepository;
            _productRepository = productRepository;
            _vendorRepository = vendorRepository;
            _currentUser = currentUser;
            _mapper = mapper;
            _stockReservationRepository = stockReservationRepository;
        }

        public async Task<ProductVariantResponse> GetVariantById(int variantId)
        {
            var variant = await _variantRepository.GetById(variantId) ?? throw new KeyNotFoundException($"Variant with ID {variantId} not found.");
            return _mapper.Map<ProductVariantResponse>(variant);
        }

        private async Task ValidateProductVariant(int productId, AddProductVariantRequest request){
            if(request.AvailableValues == null || request.AvailableValues.Count == 0)   throw new ValidationException("Product Variants must contain values ");
            var existingVariants = await _variantRepository.GetVariantsByProductId(productId);
            foreach (var existing in existingVariants)
            {
                if (existing.AvailableValues.Count == request.AvailableValues.Count && existing.AvailableValues.All(kvp => 
                        request.AvailableValues.TryGetValue(kvp.Key, out var val) && val == kvp.Value))
                {
                    throw new UniquenessViolationException("A variant with the same values already exists for this product.");
                }
            }
        }

        public async Task<ProductVariantResponse> AddVariant(int productId, AddProductVariantRequest request)
        {
            var product = await _productRepository.GetById(productId) ?? throw new InvalidProductException($"Product with ID {productId} not found.");
            await EnsureVendorOwns(product);

            await ValidateProductVariant(productId, request);

            var variant = _mapper.Map<ProductVariant>(request);
            variant.ProductId = productId;
            variant.IsActive = true;

            var created = await _variantRepository.Create(variant);
            return _mapper.Map<ProductVariantResponse>(created);
        }

        public async Task<ProductVariantResponse> UpdateVariant(int variantId, UpdateProductVariantRequest request)
        {
            var variant = await _variantRepository.GetById(variantId) ?? throw new InvalidProductException($"Variant with ID {variantId} not found.");
            
            var product = await _productRepository.GetById(variant.ProductId) ?? throw new InvalidProductException("Product not found.");
            await EnsureVendorOwns(product);

            if(request.AvailableValues != null && request.AvailableValues.Count == 0) 
                throw new ValidationException("Product Variants must contain values ");

            _mapper.Map(request, variant);
            var updated = await _variantRepository.Update(variantId, variant);
            return _mapper.Map<ProductVariantResponse>(updated);
        }

        public async Task<bool> ArchiveVariant(int variantId)
        {
            var variant = await _variantRepository.GetById(variantId) ?? throw new InvalidProductException($"Variant with ID {variantId} not found.");
            var product = await _productRepository.GetById(variant.ProductId) ?? throw new InvalidProductException("Product not found.");
            await EnsureVendorOwns(product);

            variant.IsActive = false;
            await _variantRepository.Update(variantId, variant);
            return true;
        }

        public async Task<ProductImageResponse> AddImage(int variantId, CreateProductImageRequest request)
        {
            var variant = await _variantRepository.GetById(variantId) ?? throw new InvalidProductException($"Variant with ID {variantId} not found.");
            var product = await _productRepository.GetById(variant.ProductId) ?? throw new InvalidProductException("Product not found.");
            await EnsureVendorOwns(product);

            var existingImages = await _imageRepository.GetImagesByVariantId(variantId);
            if (existingImages != null && existingImages.Any(img => img.ImageOrder == request.ImageOrder))
            {
                throw new UniquenessViolationException($"An image with order {request.ImageOrder} already exists for this variant.");
            }

            var image = _mapper.Map<ProductImage>(request);
            image.VariantId = variantId;
            var created = await _imageRepository.Create(image);
            return _mapper.Map<ProductImageResponse>(created);
        }

        public async Task<bool> DeleteImage(int imageId)
        {
            var imageVendorId = await _imageRepository.GetVendorIdByImageId(imageId);
            var currentVendorId = await GetCurrentVendorId();
            if(imageVendorId != currentVendorId) throw new InvalidOwnershipException("Access denied! You do not own this image.");
            await _imageRepository.Delete(imageId);
            return true;
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

        public async Task DecrementStock(int variantId, int quantity)
        {
            var variant = await _variantRepository.GetById(variantId) 
                ?? throw new KeyNotFoundException($"Variant with ID {variantId} not found.");
            variant.StockQty -= quantity;
            if (variant.StockQty < 0)
                throw new InvalidOperationException($"Stock underflow for variant {variantId}.");
            await _variantRepository.Update(variantId, variant);
        }

        public async Task ReserveStock(int orderId, int variantId, int quantity)
        {
            var variant = await _variantRepository.GetById(variantId) 
                ?? throw new KeyNotFoundException($"Variant with ID {variantId} not found.");
            
            if (variant.StockQty - variant.ReservedStockQty < quantity)
            {
                throw new InsufficientStockException($"Insufficient stock for variant {variantId} to reserve. Requested: {quantity}, Available: {variant.StockQty - variant.ReservedStockQty}.");
            }

            variant.ReservedStockQty += quantity;
            await _variantRepository.Update(variantId, variant);
            await _stockReservationRepository.Reserve(orderId, variantId, quantity);
        }

        public async Task ConfirmStockReservation(int orderId)
        {
            var reservations = await _stockReservationRepository.GetActiveByOrderId(orderId);
            foreach (var reservation in reservations)
            {
                await DecrementStock(reservation.VariantId, reservation.Quantity);

                var variant = await _variantRepository.GetById(reservation.VariantId);
                if (variant != null)
                {
                    variant.ReservedStockQty -= reservation.Quantity;
                    if (variant.ReservedStockQty < 0) variant.ReservedStockQty = 0;
                    await _variantRepository.Update(reservation.VariantId, variant);
                }
            }
            await _stockReservationRepository.ReleaseByOrderId(orderId);
        }

        public async Task ReleaseStockReservation(int orderId)
        {
            var reservations = await _stockReservationRepository.GetActiveByOrderId(orderId);
            foreach (var reservation in reservations)
            {
                var variant = await _variantRepository.GetById(reservation.VariantId);
                if (variant != null)
                {
                    variant.ReservedStockQty -= reservation.Quantity;
                    if (variant.ReservedStockQty < 0) variant.ReservedStockQty = 0;
                    await _variantRepository.Update(reservation.VariantId, variant);
                }
            }
            await _stockReservationRepository.ReleaseByOrderId(orderId);
        }
    }
}

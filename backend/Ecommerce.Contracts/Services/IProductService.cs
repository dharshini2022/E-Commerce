using Ecommerce.Models.DTOs;
using Ecommerce.Models;

namespace Ecommerce.Contracts.Services
{
    public interface IProductService
    {
        Task<PageResponse<ProductResponse>> GetProductsCatalog(ProductFilterRequest query);
        Task<ICollection<ProductResponse>> SearchProducts(string searchTerm);
        Task<ProductResponse> GetProductDetails(int productId);

        Task<ICollection<ProductResponse>> GetVendorProducts();
        Task<ProductResponse> CreateProduct(CreateProductRequest request);
        Task<ProductResponse> UpdateProduct(int productId, UpdateProductRequest request);
        Task<ProductResponse> ArchiveProduct(int productId);
        Task<ProductResponse?> PublishProduct(int productId);
    }
}

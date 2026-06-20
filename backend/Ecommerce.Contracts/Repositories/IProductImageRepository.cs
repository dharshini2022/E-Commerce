using Ecommerce.Models;

namespace Ecommerce.Contracts.Repositories
{
    public interface IProductImageRepository : IRepository<int, ProductImage>
    {
        Task<ICollection<ProductImage>> GetImagesByVariantId(int variantId);
        Task<int?> GetVendorIdByImageId(int imageId);

    }
}

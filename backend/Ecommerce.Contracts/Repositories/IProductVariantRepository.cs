using Ecommerce.Models;

namespace Ecommerce.Contracts.Repositories
{
    public interface IProductVariantRepository : IRepository<int, ProductVariant>
    {
        Task<ICollection<ProductVariant>> GetVariantsByProductId(int productId);
        Task<ProductVariant?> GetDefaultVariant(int productId);
        Task<bool> DecreaseStock(int variantId, int quantity);
        Task<bool> IncreaseStock(int variantId, int quantity);
    }
}

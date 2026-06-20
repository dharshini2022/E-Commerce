using Ecommerce.Models;

namespace Ecommerce.Contracts.Repositories
{
    public interface IDuplicateProductVariantRepository : IRepository<int, DuplicateProductVariant>
    {
        Task<ICollection<DuplicateProductVariant>> GetDuplicatesByProductIdAsync(int productId);
    }
}

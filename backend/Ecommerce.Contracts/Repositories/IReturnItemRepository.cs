using Ecommerce.Models;

namespace Ecommerce.Contracts.Repositories
{
    public interface IReturnItemRepository : IRepository<int, ReturnItem>
    {
        Task<ICollection<ReturnItem>> GetReturnItemsByReturnIdAsync(int returnId);
    }
}

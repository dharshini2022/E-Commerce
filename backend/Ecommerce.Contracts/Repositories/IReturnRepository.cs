using Ecommerce.Models;

namespace Ecommerce.Contracts.Repositories
{
    public interface IReturnRepository : IRepository<int, Return>
    {
        Task<ICollection<Return>> GetReturnsByUserIdAsync(int userId);
        Task<ICollection<Return>> GetReturnsByVendorIdAsync(int vendorId);
        Task<ICollection<Return>> GetUnapprovedReturnsAsync();
        Task<Return?> GetReturnWithDetailsByIdAsync(int returnId);
    }
}

using Ecommerce.Models;

namespace Ecommerce.Contracts.Repositories
{
    public interface IVendorRepository : IRepository<int, Vendor>
    {
        Task<Vendor?> GetByUserId(int userId);
        Task<ICollection<Vendor>> GetByStoreName(string storeName);
        Task<ICollection<Vendor>> GetVendorsByStatus(VendorStatus status);
        Task<(ICollection<Vendor>, int totalCount)> GetPagedVendors(string searchTerm, int pageNumber, int pageSize);
        Task<bool> VerifyGSTUnique(string gstNumber, int id = 0);
        Task<bool> VerifyPANUnique(string panNumber, int id = 0);
        Task<Vendor?> ToggleVendorStatus(int id);
        Task<bool> VerifyEmailUnique(string email, int id);
    }
}

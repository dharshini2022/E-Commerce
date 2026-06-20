using Ecommerce.Models;

namespace Ecommerce.Contracts.Repositories
{
    public interface IVendorSettlementRepository : IRepository<int, VendorSettlement>
    {
        Task<ICollection<VendorSettlement>> GetSettlementsByVendorIdAsync(int vendorId);
        Task<ICollection<VendorSettlement>> GetSettlementsByOrderIdAsync(int orderId);
        Task<ICollection<VendorSettlement>> GetSettlementsByStatusAsync(string status);
        Task<(ICollection<VendorSettlement> Items, int TotalCount)> GetPagedSettlementsWithDetails(string? searchTerm, int pageNumber, int pageSize);
    }
}

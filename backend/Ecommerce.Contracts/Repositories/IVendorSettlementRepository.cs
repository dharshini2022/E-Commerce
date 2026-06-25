using Ecommerce.Models;

namespace Ecommerce.Contracts.Repositories
{
    public interface IVendorSettlementRepository : IRepository<int, VendorSettlement>
    {
        Task<ICollection<VendorSettlement>> GetSettlementsByVendorId(int vendorId);
        Task<ICollection<VendorSettlement>> GetSettlementsByOrderId(int orderId);
        Task<ICollection<VendorSettlement>> GetSettlementsByStatus(string status);
        Task<(ICollection<VendorSettlement> Items, int TotalCount)> GetPagedSettlementsWithDetails(string? searchTerm, int pageNumber, int pageSize);
    }
}

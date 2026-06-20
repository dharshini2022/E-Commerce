using Ecommerce.Models;
using Ecommerce.Models.DTOs;

namespace Ecommerce.Contracts.Services
{
    public interface IVendorSettlementService
    {
        Task CreateSettlementsForOrder(Order order, string chargeId, Discount? discount);
        Task<ICollection<VendorSettlementDTO>> GetVendorSettlements(int vendorId);
        Task<PageResponse<VendorSettlementDTO>> GetOverallSettlements(PageRequest request);
    }
}

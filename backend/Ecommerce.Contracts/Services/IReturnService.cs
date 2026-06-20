using Ecommerce.Models.DTOs;

namespace Ecommerce.Contracts.Services
{
    public interface IReturnService
    {
        // Customer
        Task<ReturnSummaryDTO> RequestReturn(int userId, ReturnRequest request);
        Task<ICollection<ReturnSummaryDTO>> GetUserReturns(int userId);

        // Vendor
        Task<ICollection<ReturnSummaryDTO>> GetVendorReturns(int vendorId);

        // Admin
        Task<ICollection<ReturnSummaryDTO>> GetAllReturns(int? productId, int? vendorId);
        Task<ICollection<ReturnSummaryDTO>> GetPendingReturns();
        Task<ReturnSummaryDTO> ApproveReturn(int adminUserId, int returnId, Dictionary<int, bool> itemApprovalOverrides); // itemApprovalOverrides: ReturnItemId -> Approved (true) or Rejected (false)
        Task<ReturnSummaryDTO> RejectReturn(int adminUserId, int returnId);

        // Background triggers
        Task<bool> ProcessShipmentPickupCompletion(int shipmentId); // status = Picked, IsRefunded = true, Return.Status = Picked/Completed
    }
}

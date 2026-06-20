using Ecommerce.Models;
using Ecommerce.Models.DTOs;

namespace Ecommerce.Contracts.Services
{
    public interface IVendorService
    {
        Task<VendorProfileResponse> CreateVendor(CreateVendorRequest vendor);
        Task<VendorProfileResponse> UpdateVendor(UpdateVendorRequest vendor);
        Task<VendorStatusResponse> ToggleVendorStatus(int id);
        Task<PageResponse<VendorProfileResponse>> GetAllVendors(PageRequest query);
        Task<VendorProfileResponse?> GetVendorByUserId(int userId);
        Task<VendorProfileResponse?> GetVendorById(int id);
        Task<VendorBasicResponse?> GetVendorBasicById(int id);
        Task<ICollection<VendorProfileResponse>> GetVendorByStoreName(string storeName);
        Task<ICollection<VendorProfileResponse>> GetVendorsByStatus(string status);
        Task<VendorProfileResponse> ApproveVendor(int id);
        Task<VendorProfileResponse> CancelVendor(int id);
    }
}

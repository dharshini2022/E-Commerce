using Ecommerce.Models;

namespace Ecommerce.Contracts.Services
{
    public interface IShipmentService
    {
        // General / Vendor / Admin
        Task<Shipment> GetShipmentDetails(int shipmentId);
        Task<ICollection<Shipment>> GetVendorShipments(int vendorId);
        Task<ICollection<Shipment>> GetAllShipments(int? productId, int? vendorId);
        Task<ICollection<Shipment>> GetCustomerShipments(int userId);

        // Core Shipment Status transitions
        Task<Shipment> ScheduleShipment(int orderId, int userAddressId, decimal shippingFee);
        Task<Shipment> ScheduleReturnPickup(int returnId, int userAddressId);
        Task<bool> UpdateShipmentStatus(int shipmentId, ShipmentStatus status);
    }
}

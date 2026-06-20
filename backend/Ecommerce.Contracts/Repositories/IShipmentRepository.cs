using Ecommerce.Models;

namespace Ecommerce.Contracts.Repositories
{
    public interface IShipmentRepository : IRepository<int, Shipment>
    {
        Task<ICollection<Shipment>> GetShipmentsByVendorIdAsync(int vendorId);
        Task<Shipment?> GetShipmentByTrackingNumberAsync(string trackingNumber);
        Task<ICollection<Shipment>> GetActiveShipmentsAsync();
        Task<ICollection<Shipment>> GetShipmentsByUserIdAsync(int userId);
    }
}

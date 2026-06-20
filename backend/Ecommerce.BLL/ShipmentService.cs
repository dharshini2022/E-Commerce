using Ecommerce.Contracts.Repositories;
using Ecommerce.Contracts.Services;
using Ecommerce.Models;

namespace Ecommerce.BLL
{
    public class ShipmentService : IShipmentService
    {
        private readonly IShipmentRepository _shipmentRepository;

        public ShipmentService(IShipmentRepository shipmentRepository)
        {
            _shipmentRepository = shipmentRepository;
        }

        public async Task<Shipment> GetShipmentDetails(int shipmentId)
        {
            return await _shipmentRepository.GetById(shipmentId) 
                ?? throw new KeyNotFoundException($"Shipment {shipmentId} not found.");
        }

        public async Task<ICollection<Shipment>> GetVendorShipments(int vendorId)
        {
            return await _shipmentRepository.GetShipmentsByVendorIdAsync(vendorId);
        }

        public async Task<ICollection<Shipment>> GetAllShipments(int? productId, int? vendorId)
        {
            if (vendorId.HasValue)
            {
                return await _shipmentRepository.GetShipmentsByVendorIdAsync(vendorId.Value);
            }
            return await _shipmentRepository.GetAll();
        }

        public async Task<ICollection<Shipment>> GetCustomerShipments(int userId)
        {
            return await _shipmentRepository.GetShipmentsByUserIdAsync(userId);
        }

        public async Task<Shipment> ScheduleShipment(int orderId, int userAddressId, decimal shippingFee)
        {
            var shipment = new Shipment
            {
                UserAddressId = userAddressId,
                Status = ShipmentStatus.Pending,
                TrackingNumber = $"TRK{DateTime.Now:yyyyMMdd}{Guid.NewGuid().ToString("N")[..8].ToUpper()}",
                ShippedAt = null,
                FulfilledAt = null,
                ShippingFee = shippingFee
            };
            return await _shipmentRepository.Create(shipment);
        }

        public async Task<Shipment> ScheduleReturnPickup(int returnId, int userAddressId)
        {
            throw new NotImplementedException("Return pickup scheduling is not implemented.");
        }

        public async Task<bool> UpdateShipmentStatus(int shipmentId, ShipmentStatus status)
        {
            var shipment = await _shipmentRepository.GetById(shipmentId);
            if (shipment == null) throw new KeyNotFoundException("Shipment Not Found");

            shipment.Status = status;
            if (status == ShipmentStatus.Initiated)
            {
                shipment.ShippedAt = DateTime.Now;
                shipment.EstimatedFullfillement = DateTime.Now.AddDays(2);
            }
            else if (status == ShipmentStatus.Delivered)
            {
                shipment.FulfilledAt = DateTime.Now;
            }

            await _shipmentRepository.Update(shipmentId, shipment);
            return true;
        }
    }
}

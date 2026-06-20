using Ecommerce.Contracts.Repositories;
using Ecommerce.DAL.Context;
using Ecommerce.Models;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.DAL.Repositories
{
    public class ShipmentRepository : AbstractRepository<int, Shipment>, IShipmentRepository
    {
        private readonly AppDbContext _dbContext;

        public ShipmentRepository(AppDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public override async Task<Shipment?> GetById(int key)
        {
            return await _dbContext.Set<Shipment>()
                .Include(s => s.OrderItems)
                    .ThenInclude(oi => oi.Variant)
                        .ThenInclude(v => v.Product)
                .Include(s => s.OrderItems)
                    .ThenInclude(oi => oi.Vendor)
                .Include(s => s.UserAddress)
                .FirstOrDefaultAsync(s => s.Id == key);
        }

        public override async Task<ICollection<Shipment>> GetAll()
        {
            return await _dbContext.Set<Shipment>()
                .Include(s => s.OrderItems)
                    .ThenInclude(oi => oi.Variant)
                        .ThenInclude(v => v.Product)
                .Include(s => s.OrderItems)
                    .ThenInclude(oi => oi.Vendor)
                .Include(s => s.UserAddress)
                .ToListAsync();
        }

        public async Task<ICollection<Shipment>> GetShipmentsByVendorIdAsync(int vendorId)
        {
            return await _dbContext.Set<Shipment>()
                .Include(s => s.OrderItems)
                    .ThenInclude(oi => oi.Variant)
                        .ThenInclude(v => v.Product)
                .Include(s => s.OrderItems)
                    .ThenInclude(oi => oi.Vendor)
                .Include(s => s.UserAddress)
                .Where(s => s.OrderItems.Any(oi => oi.VendorId == vendorId))
                .ToListAsync();
        }

        public async Task<Shipment?> GetShipmentByTrackingNumberAsync(string trackingNumber)
        {
            return await _dbContext.Set<Shipment>()
                .Include(s => s.OrderItems)
                    .ThenInclude(oi => oi.Variant)
                        .ThenInclude(v => v.Product)
                .Include(s => s.OrderItems)
                    .ThenInclude(oi => oi.Vendor)
                .Include(s => s.UserAddress)
                .FirstOrDefaultAsync(s => s.TrackingNumber == trackingNumber);
        }

        public async Task<ICollection<Shipment>> GetActiveShipmentsAsync()
        {
            var activeStatuses = new[] { ShipmentStatus.Pending, ShipmentStatus.Initiated };
            return await _dbContext.Set<Shipment>()
                .Include(s => s.OrderItems)
                    .ThenInclude(oi => oi.Variant)
                        .ThenInclude(v => v.Product)
                .Include(s => s.OrderItems)
                    .ThenInclude(oi => oi.Vendor)
                .Include(s => s.UserAddress)
                .Where(s => activeStatuses.Contains(s.Status))
                .ToListAsync();
        }

        public async Task<ICollection<Shipment>> GetShipmentsByUserIdAsync(int userId)
        {
            return await _dbContext.Set<Shipment>()
                .Include(s => s.OrderItems)
                    .ThenInclude(oi => oi.Variant)
                        .ThenInclude(v => v.Product)
                .Include(s => s.OrderItems)
                    .ThenInclude(oi => oi.Vendor)
                .Include(s => s.UserAddress)
                .Where(s => s.UserAddress.UserId == userId)
                .ToListAsync();
        }
    }
}

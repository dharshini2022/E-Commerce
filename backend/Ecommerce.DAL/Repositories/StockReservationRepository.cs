using Ecommerce.Contracts.Repositories;
using Ecommerce.DAL.Context;
using Ecommerce.Models;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.DAL.Repositories
{
    public class StockReservationRepository : AbstractRepository<int, StockReservation>, IStockReservationRepository
    {
        private readonly AppDbContext _dbContext;

        public StockReservationRepository(AppDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<StockReservation> Reserve(int orderId, int variantId, int quantity)
        {
            var reservation = new StockReservation
            {
                OrderId = orderId,
                VariantId = variantId,
                Quantity = quantity,
                ReservedAt = DateTime.Now,
                IsReleased = false
            };
            _dbContext.StockReservations.Add(reservation);
            await _dbContext.SaveChangesAsync();
            return reservation;
        }

        public async Task<ICollection<StockReservation>> GetActiveByOrderId(int orderId)
        {
            return await _dbContext.StockReservations
                .Include(r => r.Variant)
                .Where(r => r.OrderId == orderId && !r.IsReleased)
                .ToListAsync();
        }

        public async Task<int> ReleaseByOrderId(int orderId)
        {
            var reservations = await _dbContext.StockReservations
                .Where(r => r.OrderId == orderId && !r.IsReleased)
                .ToListAsync();

            foreach (var reservation in reservations)
            {
                reservation.IsReleased = true;
                reservation.ReleasedAt = DateTime.Now;
            }

            await _dbContext.SaveChangesAsync();
            return reservations.Count;
        }
    }
}

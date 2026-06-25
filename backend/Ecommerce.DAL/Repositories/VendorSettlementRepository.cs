using Ecommerce.Contracts.Repositories;
using Ecommerce.DAL.Context;
using Ecommerce.Models;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.DAL.Repositories
{
    public class VendorSettlementRepository : AbstractRepository<int, VendorSettlement>, IVendorSettlementRepository
    {
        private readonly AppDbContext _dbContext;

        public VendorSettlementRepository(AppDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ICollection<VendorSettlement>> GetSettlementsByVendorId(int vendorId)
        {
            return await _dbContext.Set<VendorSettlement>()
                .Include(vs => vs.Vendor)
                .Include(vs => vs.Order)
                .Where(vs => vs.VendorId == vendorId)
                .OrderByDescending(vs => vs.SettledAt)
                .ToListAsync();
        }

        public async Task<ICollection<VendorSettlement>> GetSettlementsByOrderId(int orderId)
        {
            return await _dbContext.Set<VendorSettlement>()
                .Include(vs => vs.Vendor)
                .Where(vs => vs.OrderId == orderId)
                .ToListAsync();
        }

        public async Task<ICollection<VendorSettlement>> GetSettlementsByStatus(string status)
        {
            if (!Enum.TryParse<SettlementStatus>(status, true, out var settlementStatus))
                return new List<VendorSettlement>();

            return await _dbContext.Set<VendorSettlement>()
                .Include(vs => vs.Vendor)
                .Include(vs => vs.Order)
                .Where(vs => vs.Status == settlementStatus)
                .ToListAsync();
        }

        public async Task<(ICollection<VendorSettlement> Items, int TotalCount)> GetPagedSettlementsWithDetails(string? searchTerm, int pageNumber, int pageSize)
        {
            var query = _dbContext.Set<VendorSettlement>()
                .Include(s => s.Vendor)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var search = searchTerm.ToLower();
                query = query.Where(s => s.Vendor.StoreName.ToLower().Contains(search) || 
                                         (s.TransactionReference != null && s.TransactionReference.ToLower().Contains(search)));
            }

            int totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(s => s.SettledAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }
    }
}

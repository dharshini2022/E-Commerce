using Ecommerce.Contracts.Repositories;
using Ecommerce.DAL.Context;
using Ecommerce.Models;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.DAL.Repositories
{
    public class ReturnRepository : AbstractRepository<int, Return>, IReturnRepository
    {
        private readonly AppDbContext _dbContext;

        public ReturnRepository(AppDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        private IQueryable<Return> FullReturnQuery() =>
            _dbContext.Returns
                .Include(r => r.Order)
                    .ThenInclude(o => o.User)
                .Include(r => r.Items)
                    .ThenInclude(ri => ri.OrderItem)
                        .ThenInclude(oi => oi.Variant)
                            .ThenInclude(v => v.Product)
                .Include(r => r.Shipment)
                .Include(r => r.Payment);

        public async Task<ICollection<Return>> GetReturnsByUserIdAsync(int userId)
        {
            return await FullReturnQuery()
                .Where(r => r.Order.UserId == userId)
                .OrderByDescending(r => r.RequestedAt)
                .ToListAsync();
        }

        public async Task<ICollection<Return>> GetReturnsByVendorIdAsync(int vendorId)
        {
            return await FullReturnQuery()
                .Where(r => r.Items.Any(ri => ri.OrderItem.VendorId == vendorId))
                .OrderByDescending(r => r.RequestedAt)
                .ToListAsync();
        }

        public async Task<ICollection<Return>> GetUnapprovedReturnsAsync()
        {
            return await FullReturnQuery()
                .Where(r => r.Status == ReturnStatus.Requested)
                .OrderByDescending(r => r.RequestedAt)
                .ToListAsync();
        }

        public async Task<Return?> GetReturnWithDetailsByIdAsync(int returnId)
        {
            return await FullReturnQuery()
                .FirstOrDefaultAsync(r => r.Id == returnId);
        }
    }
}

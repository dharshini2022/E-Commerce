using Ecommerce.Contracts.Repositories;
using Ecommerce.DAL.Context;
using Ecommerce.Models;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.DAL.Repositories
{
    public class ReturnItemRepository : AbstractRepository<int, ReturnItem>, IReturnItemRepository
    {
        private readonly AppDbContext _dbContext;

        public ReturnItemRepository(AppDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ICollection<ReturnItem>> GetReturnItemsByReturnIdAsync(int returnId)
        {
            return await _dbContext.ReturnItems
                .Include(ri => ri.OrderItem)
                    .ThenInclude(oi => oi.Variant)
                        .ThenInclude(v => v.Product)
                .Where(ri => ri.ReturnId == returnId)
                .ToListAsync();
        }
    }
}

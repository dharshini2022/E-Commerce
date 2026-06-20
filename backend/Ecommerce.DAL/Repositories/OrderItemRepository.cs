using Ecommerce.Contracts.Repositories;
using Ecommerce.DAL.Context;
using Ecommerce.Models;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.DAL.Repositories
{
    public class OrderItemRepository : AbstractRepository<int, OrderItem>, IOrderItemRepository
    {
        private readonly AppDbContext _dbContext;

        public OrderItemRepository(AppDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ICollection<OrderItem>> GetOrderItemsByOrderId(int orderId)
        {
            return await _dbContext.OrderItems
                .Include(oi => oi.Variant)
                    .ThenInclude(v => v.Product)
                .Include(oi => oi.Vendor)
                .Where(oi => oi.OrderId == orderId)
                .ToListAsync();
        }
        public async Task CreateRange(List<OrderItem> orderItems)
        {
            _dbContext.OrderItems.AddRange(orderItems);
            await _dbContext.SaveChangesAsync();
        }
    }
}

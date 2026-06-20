using Ecommerce.Contracts.Repositories;
using Ecommerce.DAL.Context;
using Ecommerce.Models;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.DAL.Repositories
{
    public class OrderRepository : AbstractRepository<int, Order>, IOrderRepository
    {
        private readonly AppDbContext _dbContext;

        public OrderRepository(AppDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        private IQueryable<Order> FullOrderQuery() =>
            _dbContext.Orders
                .Include(o => o.User)
                .Include(o => o.Items)
                    .ThenInclude(oi => oi.Variant)
                        .ThenInclude(v => v.Product)
                .Include(o => o.Items)
                    .ThenInclude(oi => oi.Vendor)
                .Include(o => o.Payment)
                .Include(o => o.Discount);

        public async Task<Order?> GetForPayment(int orderId)
        {
            return await _dbContext.Orders.Include(o => o.Id == orderId)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Variant)
                .Include(o => o.Payment)
                .FirstOrDefaultAsync();
        }

        public async Task<ICollection<Order>> GetOrdersByUserId(int userId)
        {
            return await FullOrderQuery()
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.PlacedAt)
                .ToListAsync();
        }

        public async Task<ICollection<Order>> GetOrdersByVendorId(int vendorId)
        {
            return await FullOrderQuery()
                .Where(o => o.Items.Any(oi => oi.VendorId == vendorId))
                .OrderByDescending(o => o.PlacedAt)
                .ToListAsync();
        }

        public async Task<ICollection<Order>> GetOrdersByProductIdAsync(int productId)
        {
            return await FullOrderQuery()
                .Where(o => o.Items.Any(oi => oi.Variant.ProductId == productId))
                .OrderByDescending(o => o.PlacedAt)
                .ToListAsync();
        }

        public async Task<Order?> GetOrderWithDetailsById(int orderId)
        {
            return await FullOrderQuery()
                .FirstOrDefaultAsync(o => o.Id == orderId);
        }

        public async Task<ICollection<Order>> GetActiveVendorOrdersAsync(int vendorId)
        {
            var activeStatuses = new[] { OrderStatus.Confirmed, OrderStatus.Shipped };
            return await FullOrderQuery()
                .Where(o => o.Items.Any(oi => oi.VendorId == vendorId)
                         && activeStatuses.Contains(o.Status))
                .OrderByDescending(o => o.PlacedAt)
                .ToListAsync();
        }

        public async Task<Order?> GetOrderByStripeIntentIdAsync(string stripePaymentIntentId)
        {
            return await FullOrderQuery()
                .FirstOrDefaultAsync(o => o.StripePaymentIntentId == stripePaymentIntentId);
        }

        public async Task<ICollection<Order>> GetAllOrdersWithDetails()
        {
            return await FullOrderQuery()
                .OrderByDescending(o => o.PlacedAt)
                .ToListAsync();
        }

        public async Task<(ICollection<Order> Items, int TotalCount)> GetPagedOrders(int pageNumber, int pageSize, string? searchTerm)
        {
            var query = FullOrderQuery();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                if (System.Enum.TryParse<OrderStatus>(searchTerm, true, out var status))
                {
                    query = query.Where(o => o.Status == status);
                }
                else
                {
                    return (new List<Order>(), 0);
                }
            }

            query = query.OrderByDescending(o => o.PlacedAt);

            int totalCount = await query.CountAsync();
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }
    }
}

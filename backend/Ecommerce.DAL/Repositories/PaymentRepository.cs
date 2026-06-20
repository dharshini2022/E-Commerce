using Ecommerce.Contracts.Repositories;
using Ecommerce.DAL.Context;
using Ecommerce.Models;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.DAL.Repositories
{
    public class PaymentRepository : AbstractRepository<int, Payment>, IPaymentRepository
    {
        private readonly AppDbContext _dbContext;

        public PaymentRepository(AppDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Payment?> GetByTransactionIdAsync(string transactionId)
        {
            return await _dbContext.Set<Payment>()
                .FirstOrDefaultAsync(p => p.TransactionId == transactionId);
        }

        public async Task<ICollection<Payment>> GetPaymentHistoryByUserIdAsync(int userId)
        {
            return await _dbContext.Set<Payment>()
                .Include(p => p.Order)
                .Where(p => p.Order != null && p.Order.UserId == userId)
                .OrderByDescending(p => p.PaidAt)
                .ToListAsync();
        }

        public async Task<ICollection<Payment>> GetPaymentsPagedAsync(int pageNumber, int pageSize, string? statusFilter)
        {
            var query = _dbContext.Set<Payment>()
                .Include(p => p.Order)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(statusFilter) &&
                Enum.TryParse<PaymentStatus>(statusFilter, true, out var status))
            {
                query = query.Where(p => p.Status == status);
            }

            return await query
                .OrderByDescending(p => p.PaidAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<Payment?> GetByStripeIntentIdAsync(string stripePaymentIntentId)
        {
            return await _dbContext.Set<Payment>()
                .FirstOrDefaultAsync(p => p.StripePaymentIntentId == stripePaymentIntentId);
        }

        public async Task<(ICollection<Payment> Items, int TotalCount)> GetPagedPaymentsWithDetails(string? searchTerm, int pageNumber, int pageSize)
        {
            var query = _dbContext.Set<Payment>()
                .Include(p => p.Order)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var search = searchTerm.ToLower();
                query = query.Where(p => p.TransactionId.ToLower().Contains(search) || 
                                         (p.StripePaymentIntentId != null && p.StripePaymentIntentId.ToLower().Contains(search)));
            }

            int totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(p => p.PaidAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }
    }
}

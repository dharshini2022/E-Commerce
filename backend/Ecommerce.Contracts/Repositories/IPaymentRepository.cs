using Ecommerce.Models;

namespace Ecommerce.Contracts.Repositories
{
    public interface IPaymentRepository : IRepository<int, Payment>
    {
        Task<Payment?> GetByTransactionIdAsync(string transactionId);
        Task<ICollection<Payment>> GetPaymentHistoryByUserIdAsync(int userId);
        Task<ICollection<Payment>> GetPaymentsPagedAsync(int pageNumber, int pageSize, string? statusSort);

       
        Task<Payment?> GetByStripeIntentIdAsync(string stripePaymentIntentId);
        Task<(ICollection<Payment> Items, int TotalCount)> GetPagedPaymentsWithDetails(string? searchTerm, int pageNumber, int pageSize);
    }
}

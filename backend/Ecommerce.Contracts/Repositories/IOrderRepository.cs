using Ecommerce.Models;

namespace Ecommerce.Contracts.Repositories
{
    public interface IOrderRepository : IRepository<int, Order>
    {
        Task<ICollection<Order>> GetOrdersByUserId(int userId);
        Task<ICollection<Order>> GetOrdersByVendorId(int vendorId);
        Task<ICollection<Order>> GetOrdersByProductIdAsync(int productId);
        Task<Order?> GetOrderWithDetailsById(int orderId);
        Task<ICollection<Order>> GetActiveVendorOrdersAsync(int vendorId);
        Task<Order?> GetForPayment(int orderId);
        Task<Order?> GetOrderByStripeIntentIdAsync(string stripePaymentIntentId);
        Task<ICollection<Order>> GetAllOrdersWithDetails();
        Task<(ICollection<Order> Items, int TotalCount)> GetPagedOrders(int pageNumber, int pageSize, string? searchTerm);
    }
}

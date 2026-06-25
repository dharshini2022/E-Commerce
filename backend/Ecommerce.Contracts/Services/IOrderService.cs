using Ecommerce.Models;
using Ecommerce.Models.DTOs;

namespace Ecommerce.Contracts.Services
{
    public interface IOrderService
    {
        Task<OrderResponse> CreateOrder(CreateOrderRequest request);
        Task<OrderSummaryResponse> GetOrderDetails(int orderId);
        Task<ICollection<OrderSummaryResponse>> GetMyOrders();
        Task<ICollection<OrderSummaryResponse>> GetVendorOrders(int? vendorId = null);
        Task<PageResponse<OrderSummaryResponse>> GetAllOrders(OrderFilterRequest query);
    }
}

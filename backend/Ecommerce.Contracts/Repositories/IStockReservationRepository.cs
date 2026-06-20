using Ecommerce.Models;

namespace Ecommerce.Contracts.Repositories
{
    public interface IStockReservationRepository : IRepository<int, StockReservation>
    {
      
        Task<StockReservation> Reserve(int orderId, int variantId, int quantity);

       
        Task<ICollection<StockReservation>> GetActiveByOrderId(int orderId);

        
        Task<int> ReleaseByOrderId(int orderId);
    }
}

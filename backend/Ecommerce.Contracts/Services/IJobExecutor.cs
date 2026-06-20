using System.Threading.Tasks;

namespace Ecommerce.Contracts.Services
{
    public interface IJobExecutor
    {
        Task DeliverOrder(int orderId);
        Task ReleaseStock(int orderId);
    }
}

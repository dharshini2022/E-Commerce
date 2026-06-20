using Ecommerce.Models;

namespace Ecommerce.Contracts.Repositories
{
    public interface INotificationRepository : IRepository<int, Notification>
    {
        Task<ICollection<Notification>> GetNotificationsByUserIdAsync(int userId);
        Task<int> GetUnreadCountByUserIdAsync(int userId);
        Task<bool> MarkAllAsReadAsync(int userId);
    }
}

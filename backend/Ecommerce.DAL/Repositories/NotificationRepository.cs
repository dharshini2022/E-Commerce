using Ecommerce.Contracts.Repositories;
using Ecommerce.DAL.Context;
using Ecommerce.Models;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.DAL.Repositories
{
    public class NotificationRepository : AbstractRepository<int, Notification>, INotificationRepository
    {
        private readonly AppDbContext _dbContext;

        public NotificationRepository(AppDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ICollection<Notification>> GetNotificationsByUserIdAsync(int userId)
        {
            return await _dbContext.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<int> GetUnreadCountByUserIdAsync(int userId)
        {
            return await _dbContext.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);
        }

        public async Task<bool> MarkAllAsReadAsync(int userId)
        {
            var notifications = await _dbContext.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            if (!notifications.Any()) return true;

            foreach (var n in notifications)
                n.IsRead = true;

            await _dbContext.SaveChangesAsync();
            return true;
        }
    }
}

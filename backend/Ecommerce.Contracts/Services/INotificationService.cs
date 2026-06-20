using Ecommerce.Models;

namespace Ecommerce.Contracts.Services
{
    public interface INotificationService
    {
        Task<bool> SendEmailVerificationOtp(string email, string otp);
        Task<bool> SendEmailConfirmation(string email, string title, string message);

        Task<Notification> CreateNotification(int userId, NotificationType type, NotificationLevel level, string title, string message);
        Task<ICollection<Notification>> GetUserNotifications(int userId);
        Task<bool> MarkNotificationAsRead(int notificationId);
        Task<bool> MarkAllNotificationsAsRead(int userId);
    }
}

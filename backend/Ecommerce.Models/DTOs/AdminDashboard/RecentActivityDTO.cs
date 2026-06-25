using System.Collections.Generic;

namespace Ecommerce.Models.DTOs
{
    public class RecentActivityDTO
    {
        public ICollection<RecentOrderDTO> RecentOrders { get; set; } = [];
        public ICollection<RecentNotificationDTO> RecentNotifications { get; set; } = [];
    }
}

using System;

namespace Ecommerce.Contracts.Services
{
    public interface IBackgroundJobScheduler
    {
        void ScheduleDelivery(int orderId, TimeSpan delay);
        void ScheduleStockRelease(int orderId, TimeSpan delay);
    }
}

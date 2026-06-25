using System;
using Hangfire;
using Ecommerce.Contracts.Services;
using Ecommerce.Contracts.Services;

namespace Ecommerce.BLL
{
    public class HangfireBackgroundJobScheduler : IBackgroundJobScheduler
    {
        private readonly IBackgroundJobClient _backgroundJobClient;

        public HangfireBackgroundJobScheduler(IBackgroundJobClient backgroundJobClient)
        {
            _backgroundJobClient = backgroundJobClient;
        }

        public void ScheduleDelivery(int orderId, TimeSpan delay)
        {
            _backgroundJobClient.Schedule<IJobExecutor>(
                executor => executor.DeliverOrder(orderId),
                delay);
        }

        public void ScheduleStockRelease(int orderId, TimeSpan delay)
        {
            _backgroundJobClient.Schedule<IJobExecutor>(
                executor => executor.ReleaseStock(orderId),
                delay);
        }
    }
}

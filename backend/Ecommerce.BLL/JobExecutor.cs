using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ecommerce.Contracts.Repositories;
using Ecommerce.Contracts.Services;
using Ecommerce.Models;
using Ecommerce.DAL.Context;

namespace Ecommerce.BLL
{
    public class JobExecutor : IJobExecutor
    {
        private readonly IShipmentService _shipmentService;
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderItemRepository _orderItemRepository;
        private readonly IProductVariantService _productVariantService;
        private readonly IPaymentService _paymentService;
        private readonly AppDbContext _dbContext;

        public JobExecutor(IShipmentService shipmentService, IOrderRepository orderRepository, IOrderItemRepository orderItemRepository, IProductVariantService productVariantService, IPaymentService paymentService, AppDbContext dbContext)
        {
            _shipmentService = shipmentService;
            _orderRepository = orderRepository;
            _orderItemRepository = orderItemRepository;
            _productVariantService = productVariantService;
            _paymentService = paymentService;
            _dbContext = dbContext;
        }

        public async Task DeliverOrder(int orderId)
        {
            var orderItems = await _orderItemRepository.GetOrderItemsByOrderId(orderId);
            var shipmentIds = orderItems
                .Where(oi => oi.ShipmentId.HasValue)
                .Select(oi => oi.ShipmentId!.Value)
                .Distinct()
                .ToList();

            foreach (var shipmentId in shipmentIds)
            {
                await _shipmentService.UpdateShipmentStatus(shipmentId, ShipmentStatus.Delivered);
            }

            var order = await _orderRepository.GetById(orderId);
            if (order != null)
            {
                order.Status = OrderStatus.Delivered;
                await _orderRepository.Update(orderId, order);
            }
        }

        public async Task ReleaseStock(int orderId)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var order = await _orderRepository.GetOrderWithDetailsById(orderId);
                if (order != null && order.Status == OrderStatus.PendingPayment)
                {
                    await _productVariantService.ReleaseStockReservation(orderId);

                    order.Status = OrderStatus.PaymentFailed;
                    order.OrderPaymentStatus = PaymentStatus.Failed;
                    await _orderRepository.Update(orderId, order);

                    if (order.Payment != null)
                    {
                        await _paymentService.UpdatePaymentToFailed(order.Payment);
                    }

                    await transaction.CommitAsync();
                }
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}

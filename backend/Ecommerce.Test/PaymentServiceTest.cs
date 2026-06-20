using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Ecommerce.BLL;
using Ecommerce.Contracts.Repositories;
using Ecommerce.Contracts.Services;
using Ecommerce.DAL.Context;
using Ecommerce.Models;
using Ecommerce.Models.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;

namespace Ecommerce.Test
{
    [TestFixture]
    public class PaymentServiceTest
    {
        private Mock<IPaymentRepository> _mockPaymentRepo;
        private Mock<AppDbContext> _mockDbContext;
        private Mock<DatabaseFacade> _mockDatabaseFacade;
        private Mock<IDbContextTransaction> _mockTransaction;
        private Mock<ICurrentUserService> _mockCurrentUser;
        private Mock<IOrderRepository> _mockOrderRepo;
        private Mock<IStripeService> _mockStripeService;
        private Mock<IShipmentService> _mockShipmentService;
        private Mock<IVendorSettlementService> _mockVendorSettlementService;
        private Mock<IProductVariantService> _mockVariantService;
        private Mock<ICartRepository> _mockCartRepo;
        private Mock<IServiceProvider> _mockServiceProvider;
        private Mock<IMapper> _mockMapper;
        private Mock<IOrderItemRepository> _mockOrderItemRepo;
        private Mock<IEmailService> _mockEmailService;
        private Mock<IBackgroundJobScheduler> _mockBackgroundJobScheduler;
        private PaymentService _paymentService;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _mockDbContext = new Mock<AppDbContext>(options);
            _mockDatabaseFacade = new Mock<DatabaseFacade>(_mockDbContext.Object);
            _mockTransaction = new Mock<IDbContextTransaction>();

            _mockDbContext.Setup(x => x.Database).Returns(_mockDatabaseFacade.Object);
            _mockDatabaseFacade.Setup(d => d.BeginTransactionAsync(It.IsAny<CancellationToken>()))
                               .ReturnsAsync(_mockTransaction.Object);

            _mockPaymentRepo = new Mock<IPaymentRepository>();
            _mockCurrentUser = new Mock<ICurrentUserService>();
            _mockOrderRepo = new Mock<IOrderRepository>();
            _mockStripeService = new Mock<IStripeService>();
            _mockShipmentService = new Mock<IShipmentService>();
            _mockVendorSettlementService = new Mock<IVendorSettlementService>();
            _mockVariantService = new Mock<IProductVariantService>();
            _mockCartRepo = new Mock<ICartRepository>();
            _mockMapper = new Mock<IMapper>();
            _mockEmailService = new Mock<IEmailService>();
            _mockBackgroundJobScheduler = new Mock<IBackgroundJobScheduler>();

            // Setup ServiceProvider and ServiceScope
            var mockScope = new Mock<IServiceScope>();
            var mockScopeProvider = new Mock<IServiceProvider>();
            mockScope.Setup(s => s.ServiceProvider).Returns(mockScopeProvider.Object);

            mockScopeProvider.Setup(p => p.GetService(typeof(IShipmentService))).Returns(_mockShipmentService.Object);
            mockScopeProvider.Setup(p => p.GetService(typeof(IOrderRepository))).Returns(_mockOrderRepo.Object);
            _mockOrderItemRepo = new Mock<IOrderItemRepository>();
            mockScopeProvider.Setup(p => p.GetService(typeof(IOrderItemRepository))).Returns(_mockOrderItemRepo.Object);

            var mockScopeFactory = new Mock<IServiceScopeFactory>();
            mockScopeFactory.Setup(f => f.CreateScope()).Returns(mockScope.Object);

            _mockServiceProvider = new Mock<IServiceProvider>();
            _mockServiceProvider.Setup(p => p.GetService(typeof(IServiceScopeFactory))).Returns(mockScopeFactory.Object);

            _paymentService = new PaymentService(
                _mockPaymentRepo.Object,
                _mockDbContext.Object,
                _mockCurrentUser.Object,
                _mockOrderRepo.Object,
                _mockStripeService.Object,
                _mockShipmentService.Object,
                _mockVendorSettlementService.Object,
                _mockVariantService.Object,
                _mockCartRepo.Object,
                _mockServiceProvider.Object,
                _mockMapper.Object,
                _mockEmailService.Object,
                _mockBackgroundJobScheduler.Object
            );

            _paymentService.DeliveryScheduleDelay = TimeSpan.Zero;
        }

        [Test]
        public async Task CreatePendingPayment_ShouldCreatePayment()
        {
           
            var payment = new Payment { Amount = 100.00m };
            _mockPaymentRepo.Setup(r => r.Create(It.IsAny<Payment>())).ReturnsAsync(payment);

            
            var result = await _paymentService.CreatePendingPayment(100.00m);

            
            Assert.That(result, Is.EqualTo(payment));
            _mockPaymentRepo.Verify(r => r.Create(It.IsAny<Payment>()), Times.Once);
        }

        [Test]
        public async Task UpdatePaymentToFailed_ShouldUpdateStatus()
        {
           
            var payment = new Payment { Id = 1, Status = PaymentStatus.Pending };
            _mockPaymentRepo.Setup(r => r.Update(1, payment)).ReturnsAsync(payment);

            
            await _paymentService.UpdatePaymentToFailed(payment);

            
            Assert.That(payment.Status, Is.EqualTo(PaymentStatus.Failed));
            _mockPaymentRepo.Verify(r => r.Update(1, payment), Times.Once);
        }

        [Test]
        public void MakePayment_ShouldThrowException_WhenOrderNotFound()
        {
           
            _mockCurrentUser.Setup(u => u.UserId).Returns(1);
            _mockOrderRepo.Setup(r => r.GetOrderWithDetailsById(10)).ReturnsAsync((Order?)null);
            var request = new MakePaymentRequest { PaymentMethodId = "pm_123" };

            Assert.ThrowsAsync<KeyNotFoundException>(async () => await _paymentService.MakePayment(10, request));
        }

        [Test]
        public void MakePayment_ShouldThrowException_WhenOrderNotOwnedByUser()
        {
           
            _mockCurrentUser.Setup(u => u.UserId).Returns(1);
            var order = new Order { UserId = 2 };
            _mockOrderRepo.Setup(r => r.GetOrderWithDetailsById(10)).ReturnsAsync(order);
            var request = new MakePaymentRequest { PaymentMethodId = "pm_123" };

            Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await _paymentService.MakePayment(10, request));
        }

        [Test]
        public void MakePayment_ShouldThrowException_WhenOrderStatusIsNotPendingPayment()
        {
           
            _mockCurrentUser.Setup(u => u.UserId).Returns(1);
            var order = new Order { UserId = 1, Status = OrderStatus.Confirmed };
            _mockOrderRepo.Setup(r => r.GetOrderWithDetailsById(10)).ReturnsAsync(order);
            var request = new MakePaymentRequest { PaymentMethodId = "pm_123" };

            Assert.ThrowsAsync<InvalidOperationException>(async () => await _paymentService.MakePayment(10, request));
        }

        [Test]
        public async Task MakePayment_ShouldConfirmOrder_WhenStripeSucceeds()
        {
           
            _mockCurrentUser.Setup(u => u.UserId).Returns(1);
            
            var payment = new Payment { Id = 50, Status = PaymentStatus.Pending };
            var order = new Order 
            { 
                Id = 10, 
                UserId = 1, 
                Status = OrderStatus.PendingPayment, 
                Total = 150.00m,
                Payment = payment,
                Items = new List<OrderItem> { new OrderItem { ShipmentId = 70 } }
            };
            _mockOrderRepo.Setup(r => r.GetOrderWithDetailsById(10)).ReturnsAsync(order);

            var charge = new StripeChargeResponse { Status = "succeeded", PaymentIntentId = "pi_123" };
            _mockStripeService.Setup(s => s.MakeStripeCharge(150.00m, "inr", "pm_123", "10")).ReturnsAsync(charge);

            _mockPaymentRepo.Setup(r => r.Update(50, payment)).ReturnsAsync(payment);
            _mockOrderRepo.Setup(r => r.Update(10, order)).ReturnsAsync(order);
            _mockShipmentService.Setup(s => s.UpdateShipmentStatus(70, ShipmentStatus.Initiated)).ReturnsAsync(true);
            _mockVendorSettlementService.Setup(s => s.CreateSettlementsForOrder(order, "pi_123", It.IsAny<Discount>())).Returns(Task.CompletedTask);
            _mockVariantService.Setup(s => s.ConfirmStockReservation(10)).Returns(Task.CompletedTask);
            _mockCartRepo.Setup(r => r.GetCartByUserId(1)).ReturnsAsync(new Cart { Id = 100 });
            _mockCartRepo.Setup(r => r.ClearCart(100)).Returns(Task.CompletedTask);

            var orderItems = new List<OrderItem> { new OrderItem { ShipmentId = 70 } };
            _mockOrderItemRepo.Setup(r => r.GetOrderItemsByOrderId(10)).ReturnsAsync(orderItems);
            _mockOrderRepo.Setup(r => r.GetById(10)).ReturnsAsync(order);

            var request = new MakePaymentRequest { PaymentMethodId = "pm_123" };
            _paymentService.DeliveryScheduleDelay = TimeSpan.FromMinutes(5);

            
            var result = await _paymentService.MakePayment(10, request);

            // Wait a brief moment for background Task.Run to execute
            await Task.Delay(100);
            
            Assert.That(result.Success, Is.True);
            Assert.That(result.ChargeId, Is.EqualTo("pi_123"));
            Assert.That(order.Status, Is.EqualTo(OrderStatus.Confirmed));
            _mockTransaction.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
            _mockEmailService.Verify(e => e.SendOrderConfirmationEmail(order), Times.Once);
            _mockBackgroundJobScheduler.Verify(s => s.ScheduleDelivery(10, It.IsAny<TimeSpan>()), Times.Once);
        }

        [Test]
        public async Task MakePayment_ShouldSucceed_WhenEmailSendingThrowsException()
        {
            _mockCurrentUser.Setup(u => u.UserId).Returns(1);
            
            var payment = new Payment { Id = 50, Status = PaymentStatus.Pending };
            var order = new Order 
            { 
                Id = 10, 
                UserId = 1, 
                Status = OrderStatus.PendingPayment, 
                Total = 150.00m,
                Payment = payment,
                Items = new List<OrderItem> { new OrderItem { ShipmentId = 70 } }
            };
            _mockOrderRepo.Setup(r => r.GetOrderWithDetailsById(10)).ReturnsAsync(order);

            var charge = new StripeChargeResponse { Status = "succeeded", PaymentIntentId = "pi_123" };
            _mockStripeService.Setup(s => s.MakeStripeCharge(150.00m, "inr", "pm_123", "10")).ReturnsAsync(charge);

            _mockPaymentRepo.Setup(r => r.Update(50, payment)).ReturnsAsync(payment);
            _mockOrderRepo.Setup(r => r.Update(10, order)).ReturnsAsync(order);
            _mockShipmentService.Setup(s => s.UpdateShipmentStatus(70, ShipmentStatus.Initiated)).ReturnsAsync(true);
            _mockVendorSettlementService.Setup(s => s.CreateSettlementsForOrder(order, "pi_123", It.IsAny<Discount>())).Returns(Task.CompletedTask);
            _mockVariantService.Setup(s => s.ConfirmStockReservation(10)).Returns(Task.CompletedTask);
            _mockCartRepo.Setup(r => r.GetCartByUserId(1)).ReturnsAsync(new Cart { Id = 100 });
            _mockCartRepo.Setup(r => r.ClearCart(100)).Returns(Task.CompletedTask);

            var orderItems = new List<OrderItem> { new OrderItem { ShipmentId = 70 } };
            _mockOrderItemRepo.Setup(r => r.GetOrderItemsByOrderId(10)).ReturnsAsync(orderItems);
            _mockOrderRepo.Setup(r => r.GetById(10)).ReturnsAsync(order);

            _mockEmailService.Setup(e => e.SendOrderConfirmationEmail(order)).ThrowsAsync(new Exception("SMTP connection timed out"));

            var request = new MakePaymentRequest { PaymentMethodId = "pm_123" };
            _paymentService.DeliveryScheduleDelay = TimeSpan.FromMinutes(5);

            var result = await _paymentService.MakePayment(10, request);

            await Task.Delay(100);
            
            Assert.That(result.Success, Is.True);
            Assert.That(result.ChargeId, Is.EqualTo("pi_123"));
            Assert.That(order.Status, Is.EqualTo(OrderStatus.Confirmed));
            _mockTransaction.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
            _mockEmailService.Verify(e => e.SendOrderConfirmationEmail(order), Times.Once);
            _mockBackgroundJobScheduler.Verify(s => s.ScheduleDelivery(10, It.IsAny<TimeSpan>()), Times.Once);
        }

        [Test]
        public async Task MakePayment_ShouldFailOrder_WhenStripeFails()
        {
           
            _mockCurrentUser.Setup(u => u.UserId).Returns(1);
            
            var payment = new Payment { Id = 50, Status = PaymentStatus.Pending };
            var order = new Order 
            { 
                Id = 10, 
                UserId = 1, 
                Status = OrderStatus.PendingPayment, 
                Total = 150.00m,
                Payment = payment
            };
            _mockOrderRepo.Setup(r => r.GetOrderWithDetailsById(10)).ReturnsAsync(order);

            var charge = new StripeChargeResponse { Status = "failed", PaymentIntentId = null };
            _mockStripeService.Setup(s => s.MakeStripeCharge(150.00m, "inr", "pm_123", "10")).ReturnsAsync(charge);

            var request = new MakePaymentRequest { PaymentMethodId = "pm_123" };

            
            var result = await _paymentService.MakePayment(10, request);

            
            Assert.That(result.Success, Is.False);
            Assert.That(order.Status, Is.EqualTo(OrderStatus.PaymentFailed));
            _mockTransaction.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task MakePayment_ShouldFailOrder_WhenExceptionThrown()
        {
           
            _mockCurrentUser.Setup(u => u.UserId).Returns(1);
            
            var payment = new Payment { Id = 50, Status = PaymentStatus.Pending };
            var order = new Order 
            { 
                Id = 10, 
                UserId = 1, 
                Status = OrderStatus.PendingPayment, 
                Total = 150.00m,
                Payment = payment
            };
            _mockOrderRepo.Setup(r => r.GetOrderWithDetailsById(10)).ReturnsAsync(order);
            _mockStripeService.Setup(s => s.MakeStripeCharge(It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                              .ThrowsAsync(new Exception("Stripe Error"));

            var request = new MakePaymentRequest { PaymentMethodId = "pm_123" };

            
            var result = await _paymentService.MakePayment(10, request);

            
            Assert.That(result.Success, Is.False);
            Assert.That(order.Status, Is.EqualTo(OrderStatus.PaymentFailed));
        }

        [Test]
        public async Task GetMyPaymentHistory_ShouldReturnMappedHistory()
        {
           
            _mockCurrentUser.Setup(u => u.UserId).Returns(1);
            var payments = new List<Payment> { new Payment() };
            _mockPaymentRepo.Setup(r => r.GetPaymentHistoryByUserIdAsync(1)).ReturnsAsync(payments);

            var expected = new List<PaymentResponseDTO> { new PaymentResponseDTO() };
            _mockMapper.Setup(m => m.Map<ICollection<PaymentResponseDTO>>(payments)).Returns(expected);

            
            var result = await _paymentService.GetMyPaymentHistory();

            
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public async Task GetOverallPaymentHistory_ShouldReturnPagedHistory()
        {
           
            var pageRequest = new PageRequest { PageNumber = 1, PageSize = 10, SearchTerm = "test" };
            var payments = new List<Payment> { new Payment() };
            _mockPaymentRepo.Setup(r => r.GetPagedPaymentsWithDetails("test", 1, 10)).ReturnsAsync((payments, 1));

            var expected = new List<PaymentResponseDTO> { new PaymentResponseDTO() };
            _mockMapper.Setup(m => m.Map<List<PaymentResponseDTO>>(payments)).Returns(expected);

            
            var result = await _paymentService.GetOverallPaymentHistory(pageRequest);

            
            Assert.That(result.Items, Is.EqualTo(expected));
            Assert.That(result.TotalCount, Is.EqualTo(1));
        }

        [Test]
        public async Task MakePayment_ShouldRollbackAndThrow_WhenConfirmOrderFails()
        {
           
            _mockCurrentUser.Setup(u => u.UserId).Returns(1);
            var payment = new Payment { Id = 50, Status = PaymentStatus.Pending };
            var order = new Order { Id = 10, UserId = 1, Status = OrderStatus.PendingPayment, Total = 150.00m, Payment = payment };
            _mockOrderRepo.Setup(r => r.GetOrderWithDetailsById(10)).ReturnsAsync(order);

            var charge = new StripeChargeResponse { Status = "succeeded", PaymentIntentId = "pi_123" };
            _mockStripeService.Setup(s => s.MakeStripeCharge(150.00m, "inr", "pm_123", "10")).ReturnsAsync(charge);

            var mockTransaction1 = new Mock<IDbContextTransaction>();
            var mockTransaction2 = new Mock<IDbContextTransaction>();
            var queue = new Queue<IDbContextTransaction>(new[] { mockTransaction1.Object, mockTransaction2.Object });
            _mockDatabaseFacade.Setup(d => d.BeginTransactionAsync(It.IsAny<CancellationToken>()))
                               .Returns(() => Task.FromResult(queue.Dequeue()));

            _mockPaymentRepo.Setup(r => r.Update(50, payment)).ReturnsAsync(payment);
            
            _mockOrderRepo.Setup(r => r.Update(10, It.Is<Order>(o => o.Status == OrderStatus.Confirmed))).ThrowsAsync(new Exception("DB Error"));
            _mockOrderRepo.Setup(r => r.Update(10, It.Is<Order>(o => o.Status == OrderStatus.PaymentFailed))).ReturnsAsync(order);

            var request = new MakePaymentRequest { PaymentMethodId = "pm_123" };

            
            var result = await _paymentService.MakePayment(10, request);

            
            Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Contains.Substring("Stripe error: DB Error"));
            mockTransaction1.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
            mockTransaction2.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task MakePayment_ShouldRollbackAndThrow_WhenFailOrderFails()
        {
           
            _mockCurrentUser.Setup(u => u.UserId).Returns(1);
            var payment = new Payment { Id = 50, Status = PaymentStatus.Pending };
            var order = new Order { Id = 10, UserId = 1, Status = OrderStatus.PendingPayment, Total = 150.00m, Payment = payment };
            _mockOrderRepo.Setup(r => r.GetOrderWithDetailsById(10)).ReturnsAsync(order);

            var charge = new StripeChargeResponse { Status = "failed", PaymentIntentId = "pi_123" };
            _mockStripeService.Setup(s => s.MakeStripeCharge(150.00m, "inr", "pm_123", "10")).ReturnsAsync(charge);

            var mockTransaction1 = new Mock<IDbContextTransaction>();
            var mockTransaction2 = new Mock<IDbContextTransaction>();
            var queue = new Queue<IDbContextTransaction>(new[] { mockTransaction1.Object, mockTransaction2.Object });
            _mockDatabaseFacade.Setup(d => d.BeginTransactionAsync(It.IsAny<CancellationToken>()))
                               .Returns(() => Task.FromResult(queue.Dequeue()));

            _mockOrderRepo.Setup(r => r.Update(10, order)).ThrowsAsync(new Exception("DB Error"));

            var request = new MakePaymentRequest { PaymentMethodId = "pm_123" };

            var ex = Assert.ThrowsAsync<Exception>(async () => await _paymentService.MakePayment(10, request));
            Assert.That(ex.Message, Is.EqualTo("DB Error"));
            mockTransaction1.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
            mockTransaction2.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}

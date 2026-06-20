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
using Ecommerce.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;

namespace Ecommerce.Test
{
    [TestFixture]
    public class OrderServiceTest
    {
        private Mock<AppDbContext> _mockDbContext;
        private Mock<DatabaseFacade> _mockDatabaseFacade;
        private Mock<IDbContextTransaction> _mockTransaction;
        private Mock<IOrderRepository> _mockOrderRepo;
        private Mock<IDiscountRepository> _mockDiscountRepo;
        private Mock<IVendorRepository> _mockVendorRepo;
        private Mock<ICartRepository> _mockCartRepo;
        private Mock<IOrderItemRepository> _mockOrderItemRepo;
        private Mock<IUserRepository> _mockUserRepo;
        private Mock<IDiscountService> _mockDiscountService;
        private Mock<ICartService> _mockCartService;
        private Mock<IPaymentService> _mockPaymentService;
        private Mock<ICurrentUserService> _mockCurrentUser;
        private Mock<IMapper> _mockMapper;
        private Mock<IShipmentService> _mockShipmentService;
        private Mock<IProductVariantService> _mockVariantService;
        private Mock<IServiceProvider> _mockServiceProvider;
        private Mock<IBackgroundJobScheduler> _mockBackgroundJobScheduler;
        private OrderService _orderService;

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

            _mockOrderRepo = new Mock<IOrderRepository>();
            _mockDiscountRepo = new Mock<IDiscountRepository>();
            _mockVendorRepo = new Mock<IVendorRepository>();
            _mockCartRepo = new Mock<ICartRepository>();
            _mockOrderItemRepo = new Mock<IOrderItemRepository>();
            _mockUserRepo = new Mock<IUserRepository>();
            _mockDiscountService = new Mock<IDiscountService>();
            _mockCartService = new Mock<ICartService>();
            _mockPaymentService = new Mock<IPaymentService>();
            _mockCurrentUser = new Mock<ICurrentUserService>();
            _mockMapper = new Mock<IMapper>();
            _mockShipmentService = new Mock<IShipmentService>();
            _mockVariantService = new Mock<IProductVariantService>();
            _mockBackgroundJobScheduler = new Mock<IBackgroundJobScheduler>();

            var mockScope = new Mock<IServiceScope>();
            var mockScopeProvider = new Mock<IServiceProvider>();
            mockScope.Setup(s => s.ServiceProvider).Returns(mockScopeProvider.Object);

            mockScopeProvider.Setup(p => p.GetService(typeof(IOrderRepository))).Returns(_mockOrderRepo.Object);
            mockScopeProvider.Setup(p => p.GetService(typeof(AppDbContext))).Returns(_mockDbContext.Object);
            mockScopeProvider.Setup(p => p.GetService(typeof(IProductVariantService))).Returns(_mockVariantService.Object);
            mockScopeProvider.Setup(p => p.GetService(typeof(IPaymentService))).Returns(_mockPaymentService.Object);

            var mockScopeFactory = new Mock<IServiceScopeFactory>();
            mockScopeFactory.Setup(f => f.CreateScope()).Returns(mockScope.Object);

            _mockServiceProvider = new Mock<IServiceProvider>();
            _mockServiceProvider.Setup(p => p.GetService(typeof(IServiceScopeFactory))).Returns(mockScopeFactory.Object);

            _orderService = new OrderService(
                _mockDbContext.Object,
                _mockOrderRepo.Object,
                _mockDiscountRepo.Object,
                _mockVendorRepo.Object,
                _mockCartRepo.Object,
                _mockOrderItemRepo.Object,
                _mockUserRepo.Object,
                _mockDiscountService.Object,
                _mockCartService.Object,
                _mockPaymentService.Object,
                _mockCurrentUser.Object,
                _mockMapper.Object,
                _mockShipmentService.Object,
                _mockVariantService.Object,
                _mockServiceProvider.Object,
                _mockBackgroundJobScheduler.Object
            );

            _orderService.StockReleaseDelay = TimeSpan.Zero;
        }

        [Test]
        public void CreateOrder_ShouldThrowException_WhenAddressNotFound()
        {
           
            _mockCurrentUser.Setup(u => u.UserId).Returns(1);
            _mockUserRepo.Setup(r => r.GetAllAddressByUserId(1)).ReturnsAsync(new List<UserAddress>());
            var request = new CreateOrderRequest { UserAddressId = 10 };

             
            Assert.ThrowsAsync<KeyNotFoundException>(async () => await _orderService.CreateOrder(request));
        }

        [Test]
        public async Task CreateOrder_ShouldCreateOrder_WhenRequestIsValid()
        {
           
            _mockCurrentUser.Setup(u => u.UserId).Returns(1);
            var addresses = new List<UserAddress> { new UserAddress { Id = 10, UserId = 1 } };
            _mockUserRepo.Setup(r => r.GetAllAddressByUserId(1)).ReturnsAsync(addresses);

            var cart = new Cart { Id = 100, Items = new List<CartItem> { new CartItem() } };
            _mockCartRepo.Setup(r => r.GetCartByUserId(1)).ReturnsAsync(cart);

            var product = new Product { Id = 20, VendorId = 2 };
            var variant = new ProductVariant { Id = 30, Price = 100.00m, Product = product };
            var eligibleItems = new List<CartItem>
            {
                new CartItem { VariantId = 30, Quantity = 2, Variant = variant }
            };
            _mockCartService.Setup(s => s.GetEligibleItems(1)).ReturnsAsync(eligibleItems);

            var discount = new Discount { Id = 5 };
            _mockDiscountService.Setup(s => s.ValidateDiscount("CODE", eligibleItems, 200.00m)).ReturnsAsync(discount);
            _mockDiscountService.Setup(s => s.CalculateDiscountAmount(discount, 200.00m)).ReturnsAsync(20.00m);

            var payment = new Payment { Id = 50 };
            _mockPaymentService.Setup(s => s.CreatePendingPayment(It.IsAny<decimal>())).ReturnsAsync(payment);

            var createdOrder = new Order { Id = 60, Subtotal = 200.00m, Total = 200.00m };
            _mockOrderRepo.Setup(r => r.Create(It.IsAny<Order>())).ReturnsAsync(createdOrder);

            var shipment = new Shipment { Id = 70 };
            _mockShipmentService.Setup(s => s.ScheduleShipment(60, 10, It.IsAny<decimal>())).ReturnsAsync(shipment);

            _mockOrderItemRepo.Setup(r => r.CreateRange(It.IsAny<List<OrderItem>>())).Returns(Task.CompletedTask);
            _mockVariantService.Setup(s => s.ReserveStock(60, 30, 2)).Returns(Task.CompletedTask);

            var request = new CreateOrderRequest { UserAddressId = 10, DiscountCode = "CODE" };

            
            var result = await _orderService.CreateOrder(request);

            
            Assert.That(result, Is.Not.Null);
            Assert.That(result.OrderId, Is.EqualTo(60));
            _mockTransaction.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public void CreateOrder_ShouldRollbackAndThrow_WhenTransactionFails()
        {
           
            _mockCurrentUser.Setup(u => u.UserId).Returns(1);
            var addresses = new List<UserAddress> { new UserAddress { Id = 10, UserId = 1 } };
            _mockUserRepo.Setup(r => r.GetAllAddressByUserId(1)).ReturnsAsync(addresses);
            
            var product = new Product { Id = 20, VendorId = 2 };
            var variant = new ProductVariant { Id = 30, Price = 100.00m, Product = product };
            var eligibleItems = new List<CartItem>
            {
                new CartItem { VariantId = 30, Quantity = 2, Variant = variant }
            };
            _mockCartService.Setup(s => s.GetEligibleItems(1)).ReturnsAsync(eligibleItems);
            _mockPaymentService.Setup(s => s.CreatePendingPayment(It.IsAny<decimal>())).ThrowsAsync(new Exception("BLL Error"));

            var request = new CreateOrderRequest { UserAddressId = 10 };

            Assert.ThrowsAsync<Exception>(async () => await _orderService.CreateOrder(request));
            _mockTransaction.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task ScheduleStockRelease_ShouldTriggerAndReleaseStock_WhenUnpaid()
        {
           
            _mockCurrentUser.Setup(u => u.UserId).Returns(1);
            var addresses = new List<UserAddress> { new UserAddress { Id = 10 } };
            _mockUserRepo.Setup(r => r.GetAllAddressByUserId(1)).ReturnsAsync(addresses);

            var cart = new Cart { Id = 100, Items = new List<CartItem>() };
            _mockCartRepo.Setup(r => r.GetCartByUserId(1)).ReturnsAsync(cart);

            var product = new Product { Id = 20, VendorId = 2 };
            var variant = new ProductVariant { Id = 30, Price = 100.00m, Product = product };
            var eligibleItems = new List<CartItem> { new CartItem { VariantId = 30, Quantity = 2, Variant = variant } };
            _mockCartService.Setup(s => s.GetEligibleItems(1)).ReturnsAsync(eligibleItems);

            var payment = new Payment { Id = 50 };
            _mockPaymentService.Setup(s => s.CreatePendingPayment(It.IsAny<decimal>())).ReturnsAsync(payment);

            var order = new Order { Id = 60, Status = OrderStatus.PendingPayment, Payment = payment };
            _mockOrderRepo.Setup(r => r.Create(It.IsAny<Order>())).ReturnsAsync(order);
            _mockOrderRepo.Setup(r => r.GetOrderWithDetailsById(60)).ReturnsAsync(order);

            _mockShipmentService.Setup(s => s.ScheduleShipment(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<decimal>())).ReturnsAsync(new Shipment());

            
            await _orderService.CreateOrder(new CreateOrderRequest { UserAddressId = 10 });

            _mockBackgroundJobScheduler.Verify(s => s.ScheduleStockRelease(60, It.IsAny<TimeSpan>()), Times.Once);
        }

        [Test]
        public void GetOrderDetails_ShouldThrowKeyNotFoundException_WhenOrderNotFound()
        {
           
            _mockOrderRepo.Setup(r => r.GetOrderWithDetailsById(100)).ReturnsAsync((Order?)null);

            Assert.ThrowsAsync<KeyNotFoundException>(async () => await _orderService.GetOrderDetails(100));
        }

        [Test]
        public void GetOrderDetails_CustomerAccessMismatchedUser_ThrowsUnauthorizedAccessException()
        {
           
            var order = new Order { Id = 100, UserId = 2 };
            _mockOrderRepo.Setup(r => r.GetOrderWithDetailsById(100)).ReturnsAsync(order);
            _mockCurrentUser.Setup(u => u.Role).Returns("Customer");
            _mockCurrentUser.Setup(u => u.UserId).Returns(1);

            Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await _orderService.GetOrderDetails(100));
        }

        [Test]
        public void GetOrderDetails_VendorAccessMismatchedVendor_ThrowsUnauthorizedAccessException()
        {
           
            var order = new Order 
            { 
                Id = 100, 
                UserId = 1,
                Items = new List<OrderItem> { new OrderItem { VendorId = 5 } }
            };
            _mockOrderRepo.Setup(r => r.GetOrderWithDetailsById(100)).ReturnsAsync(order);
            _mockCurrentUser.Setup(u => u.Role).Returns("Vendor");
            _mockCurrentUser.Setup(u => u.UserId).Returns(1);
            _mockVendorRepo.Setup(r => r.GetByUserId(1)).ReturnsAsync(new Vendor { Id = 6 });

            Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await _orderService.GetOrderDetails(100));
        }

        [Test]
        public async Task GetOrderDetails_ValidAccess_ReturnsOrderSummaryResponse()
        {
           
            var order = new Order { Id = 100, UserId = 1 };
            _mockOrderRepo.Setup(r => r.GetOrderWithDetailsById(100)).ReturnsAsync(order);
            _mockCurrentUser.Setup(u => u.Role).Returns("Admin");
            _mockCurrentUser.Setup(u => u.UserId).Returns(1);

            var expected = new OrderSummaryResponse { OrderId = 100 };
            _mockMapper.Setup(m => m.Map<OrderSummaryResponse>(order)).Returns(expected);

            
            var result = await _orderService.GetOrderDetails(100);

            
            Assert.That(result, Is.Not.Null);
            Assert.That(result.OrderId, Is.EqualTo(100));
        }

        [Test]
        public async Task QueryMethods_ShouldReturnMappedCollections()
        {
           
            _mockCurrentUser.Setup(u => u.UserId).Returns(1);
            var list = new List<Order> { new Order { Id = 10 } };
            _mockOrderRepo.Setup(r => r.GetOrdersByUserId(It.IsAny<int>())).ReturnsAsync(list);

            var pagedResult = (list, 1);
            _mockOrderRepo.Setup(r => r.GetPagedOrders(1, 10, "search")).ReturnsAsync(pagedResult);

            _mockCurrentUser.Setup(u => u.UserId).Returns(2);
            _mockVendorRepo.Setup(r => r.GetByUserId(2)).ReturnsAsync(new Vendor { Id = 5 });
            _mockOrderRepo.Setup(r => r.GetOrdersByVendorId(5)).ReturnsAsync(list);

            var mappedList = new List<OrderSummaryResponse> { new OrderSummaryResponse { OrderId = 10 } };
            _mockMapper.Setup(m => m.Map<ICollection<OrderSummaryResponse>>(list)).Returns(mappedList);

            var myOrders = await _orderService.GetMyOrders();
            Assert.That(myOrders.First().OrderId, Is.EqualTo(10));

            var allOrders = await _orderService.GetAllOrders(new OrderFilterRequest { PageNumber = 1, PageSize = 10, SearchTerm = "search" });
            Assert.That(allOrders.Items.First().OrderId, Is.EqualTo(10));

            var vendorOrders = await _orderService.GetVendorOrders();
            Assert.That(vendorOrders.First().OrderId, Is.EqualTo(10));
        }

        [Test]
        public async Task ScheduleStockRelease_ShouldCatchAndThrow_WhenRepositoryFails()
        {
           
            _mockCurrentUser.Setup(u => u.UserId).Returns(1);
            var addresses = new List<UserAddress> { new UserAddress { Id = 10 } };
            _mockUserRepo.Setup(r => r.GetAllAddressByUserId(1)).ReturnsAsync(addresses);

            var cart = new Cart { Id = 100, Items = new List<CartItem>() };
            _mockCartRepo.Setup(r => r.GetCartByUserId(1)).ReturnsAsync(cart);

            var product = new Product { Id = 20, VendorId = 2 };
            var variant = new ProductVariant { Id = 30, Price = 100.00m, Product = product };
            var eligibleItems = new List<CartItem> { new CartItem { VariantId = 30, Quantity = 2, Variant = variant } };
            _mockCartService.Setup(s => s.GetEligibleItems(1)).ReturnsAsync(eligibleItems);

            var payment = new Payment { Id = 50 };
            _mockPaymentService.Setup(s => s.CreatePendingPayment(It.IsAny<decimal>())).ReturnsAsync(payment);

            var order = new Order { Id = 60, Status = OrderStatus.PendingPayment, Payment = payment };
            _mockOrderRepo.Setup(r => r.Create(It.IsAny<Order>())).ReturnsAsync(order);
            
            _mockOrderRepo.Setup(r => r.GetOrderWithDetailsById(60)).ThrowsAsync(new Exception("DB Error"));

            _mockShipmentService.Setup(s => s.ScheduleShipment(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<decimal>())).ReturnsAsync(new Shipment());

            
            await _orderService.CreateOrder(new CreateOrderRequest { UserAddressId = 10 });

            _mockBackgroundJobScheduler.Verify(s => s.ScheduleStockRelease(60, It.IsAny<TimeSpan>()), Times.Once);
        }

        [Test]
        public async Task GetOrderDetails_VendorAccessOwnOrder_ReturnsOrderSummaryResponse()
        {
           
            var order = new Order 
            { 
                Id = 100, 
                UserId = 1,
                Items = new List<OrderItem> { new OrderItem { VendorId = 6 } }
            };
            _mockOrderRepo.Setup(r => r.GetOrderWithDetailsById(100)).ReturnsAsync(order);
            _mockCurrentUser.Setup(u => u.Role).Returns("Vendor");
            _mockCurrentUser.Setup(u => u.UserId).Returns(1);
            _mockVendorRepo.Setup(r => r.GetByUserId(1)).ReturnsAsync(new Vendor { Id = 6 });

            var expected = new OrderSummaryResponse { OrderId = 100 };
            _mockMapper.Setup(m => m.Map<OrderSummaryResponse>(order)).Returns(expected);

            
            var result = await _orderService.GetOrderDetails(100);

            
            Assert.That(result, Is.Not.Null);
            Assert.That(result.OrderId, Is.EqualTo(100));
        }

        [Test]
        public void GenerateTrackingNumber_ReturnsValidFormat()
        {
            var method = typeof(OrderService).GetMethod("GenerateTrackingNumber", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            var result = method?.Invoke(null, null) as string;

            
            Assert.That(result, Is.Not.Null);
            Assert.That(result.StartsWith("TRK"), Is.True);
            Assert.That(result.Length, Is.GreaterThan(10));
        }

        [Test]
        public async Task CreateOrder_ShouldCreateOrder_WithSkippedItems_WhenSomeItemsAreOutOfStock()
        {
           
            _mockCurrentUser.Setup(u => u.UserId).Returns(1);
            var addresses = new List<UserAddress> { new UserAddress { Id = 10, UserId = 1 } };
            _mockUserRepo.Setup(r => r.GetAllAddressByUserId(1)).ReturnsAsync(addresses);

            var cart = new Cart { Id = 100, Items = new List<CartItem> { new CartItem(), new CartItem() } };
            _mockCartRepo.Setup(r => r.GetCartByUserId(1)).ReturnsAsync(cart);

            var product = new Product { Id = 20, VendorId = 2 };
            var variant = new ProductVariant { Id = 30, Price = 100.00m, Product = product };
            var eligibleItems = new List<CartItem>
            {
                new CartItem { VariantId = 30, Quantity = 2, Variant = variant }
            };
            _mockCartService.Setup(s => s.GetEligibleItems(1)).ReturnsAsync(eligibleItems);

            var payment = new Payment { Id = 50 };
            _mockPaymentService.Setup(s => s.CreatePendingPayment(It.IsAny<decimal>())).ReturnsAsync(payment);

            var createdOrder = new Order { Id = 60, Subtotal = 200.00m, Total = 200.00m };
            _mockOrderRepo.Setup(r => r.Create(It.IsAny<Order>())).ReturnsAsync(createdOrder);

            var shipment = new Shipment { Id = 70 };
            _mockShipmentService.Setup(s => s.ScheduleShipment(60, 10, It.IsAny<decimal>())).ReturnsAsync(shipment);

            _mockOrderItemRepo.Setup(r => r.CreateRange(It.IsAny<List<OrderItem>>())).Returns(Task.CompletedTask);
            _mockVariantService.Setup(s => s.ReserveStock(60, 30, 2)).Returns(Task.CompletedTask);

            var request = new CreateOrderRequest { UserAddressId = 10 };

            
            var result = await _orderService.CreateOrder(request);

            
            Assert.That(result, Is.Not.Null);
            Assert.That(result.OrderId, Is.EqualTo(60));
            Assert.That(result.Message, Contains.Substring("out-of-stock item(s) were skipped"));
            _mockTransaction.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public void CreateOrder_ShouldThrowValidationException_WhenPendingOrderWithSameItemsExists()
        {
            _mockCurrentUser.Setup(u => u.UserId).Returns(1);
            var addresses = new List<UserAddress> { new UserAddress { Id = 10, UserId = 1 } };
            _mockUserRepo.Setup(r => r.GetAllAddressByUserId(1)).ReturnsAsync(addresses);

            var cart = new Cart { Id = 100, Items = new List<CartItem> { new CartItem() } };
            _mockCartRepo.Setup(r => r.GetCartByUserId(1)).ReturnsAsync(cart);

            var product = new Product { Id = 20, VendorId = 2 };
            var variant = new ProductVariant { Id = 30, Price = 100.00m, Product = product };
            var eligibleItems = new List<CartItem>
            {
                new CartItem { VariantId = 30, Quantity = 2, Variant = variant }
            };
            _mockCartService.Setup(s => s.GetEligibleItems(1)).ReturnsAsync(eligibleItems);

            var existingOrders = new List<Order>
            {
                new Order
                {
                    Id = 60,
                    UserId = 1,
                    Status = OrderStatus.PendingPayment,
                    Items = new List<OrderItem>
                    {
                        new OrderItem { VariantId = 30, Quantity = 2 }
                    }
                }
            };
            _mockOrderRepo.Setup(r => r.GetOrdersByUserId(1)).ReturnsAsync(existingOrders);

            var request = new CreateOrderRequest { UserAddressId = 10 };

            var ex = Assert.ThrowsAsync<ValidationException>(async () => await _orderService.CreateOrder(request));
            Assert.That(ex.Message, Is.EqualTo("A pending order with the same items already exists. Please complete the payment for the existing order."));
        }

        [Test]
        public void CreateOrder_ShouldThrowValidationException_WhenCartIsEmpty()
        {
            _mockCurrentUser.Setup(u => u.UserId).Returns(1);
            var addresses = new List<UserAddress> { new UserAddress { Id = 10, UserId = 1 } };
            _mockUserRepo.Setup(r => r.GetAllAddressByUserId(1)).ReturnsAsync(addresses);

            _mockCartService.Setup(s => s.GetEligibleItems(1)).ReturnsAsync(new List<CartItem>());

            var request = new CreateOrderRequest { UserAddressId = 10 };

            var ex = Assert.ThrowsAsync<ValidationException>(async () => await _orderService.CreateOrder(request));
            Assert.That(ex.Message, Is.EqualTo("Cart is empty or has no eligible items."));
        }
    }
}

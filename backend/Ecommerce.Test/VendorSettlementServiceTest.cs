using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Ecommerce.BLL;
using Ecommerce.Contracts.Repositories;
using Ecommerce.Models;
using Ecommerce.Models.DTOs;
using Moq;
using NUnit.Framework;

namespace Ecommerce.Test
{
    [TestFixture]
    public class VendorSettlementServiceTest
    {
        private Mock<IVendorSettlementRepository> _mockSettlementRepo;
        private Mock<IMapper> _mockMapper;
        private VendorSettlementService _settlementService;

        [SetUp]
        public void Setup()
        {
            _mockSettlementRepo = new Mock<IVendorSettlementRepository>();
            _mockMapper = new Mock<IMapper>();
            _settlementService = new VendorSettlementService(_mockSettlementRepo.Object, _mockMapper.Object);
        }

        [Test]
        public async Task CreateSettlementsForOrder_WithoutDiscount_ShouldCreateSettlements()
        {
           
            var order = new Order
            {
                Id = 1,
                Subtotal = 100.00m,
                DiscountAmount = 0,
                Items = new List<OrderItem>
                {
                    new OrderItem { VendorId = 10, UnitPrice = 50.00m, Quantity = 2 }
                }
            };

            _mockSettlementRepo.Setup(r => r.Create(It.IsAny<VendorSettlement>())).ReturnsAsync(new VendorSettlement());

            
            await _settlementService.CreateSettlementsForOrder(order, "pi_123", null);

            
            _mockSettlementRepo.Verify(r => r.Create(It.Is<VendorSettlement>(s => 
                s.VendorId == 10 && 
                s.GrossAmount == 100.00m && 
                s.VendorDiscountAmount == 0)), Times.Once);
        }

        [Test]
        public async Task CreateSettlementsForOrder_WithVendorDiscount_ShouldApplyDiscount()
        {
           
            var order = new Order
            {
                Id = 1,
                Subtotal = 100.00m,
                DiscountAmount = 15.00m,
                Items = new List<OrderItem>
                {
                    new OrderItem { VendorId = 10, UnitPrice = 50.00m, Quantity = 2 }
                }
            };
            var discount = new Discount { Scope = DiscountScope.Vendor, VendorId = 10 };

            _mockSettlementRepo.Setup(r => r.Create(It.IsAny<VendorSettlement>())).ReturnsAsync(new VendorSettlement());

            
            await _settlementService.CreateSettlementsForOrder(order, "pi_123", discount);

            
            _mockSettlementRepo.Verify(r => r.Create(It.Is<VendorSettlement>(s => 
                s.VendorId == 10 && 
                s.VendorDiscountAmount == 15.00m)), Times.Once);
        }

        [Test]
        public async Task CreateSettlementsForOrder_WithVendorDiscountForMismatchedVendor_ShouldNotApplyDiscount()
        {
           
            var order = new Order
            {
                Id = 1,
                Subtotal = 100.00m,
                DiscountAmount = 15.00m,
                Items = new List<OrderItem>
                {
                    new OrderItem { VendorId = 10, UnitPrice = 50.00m, Quantity = 2 }
                }
            };
            var discount = new Discount { Scope = DiscountScope.Vendor, VendorId = 11 };

            _mockSettlementRepo.Setup(r => r.Create(It.IsAny<VendorSettlement>())).ReturnsAsync(new VendorSettlement());

            
            await _settlementService.CreateSettlementsForOrder(order, "pi_123", discount);

            
            _mockSettlementRepo.Verify(r => r.Create(It.Is<VendorSettlement>(s => 
                s.VendorId == 10 && 
                s.VendorDiscountAmount == 0m)), Times.Once);
        }

        [Test]
        public async Task CreateSettlementsForOrder_WithProductDiscount_ShouldApplyProportionalDiscount()
        {
           
            var order = new Order
            {
                Id = 1,
                Subtotal = 200.00m,
                DiscountAmount = 20.00m,
                Items = new List<OrderItem>
                {
                    new OrderItem { VendorId = 10, UnitPrice = 50.00m, Quantity = 2 }, // Gross = 100, Share = 0.5
                    new OrderItem { VendorId = 11, UnitPrice = 100.00m, Quantity = 1 } // Gross = 100, Share = 0.5
                }
            };
            var discount = new Discount { Scope = DiscountScope.Product };

            _mockSettlementRepo.Setup(r => r.Create(It.IsAny<VendorSettlement>())).ReturnsAsync(new VendorSettlement());

            
            await _settlementService.CreateSettlementsForOrder(order, "pi_123", discount);

            
            _mockSettlementRepo.Verify(r => r.Create(It.Is<VendorSettlement>(s => 
                s.VendorId == 10 && 
                s.VendorDiscountAmount == 10.00m)), Times.Once);
            _mockSettlementRepo.Verify(r => r.Create(It.Is<VendorSettlement>(s => 
                s.VendorId == 11 && 
                s.VendorDiscountAmount == 10.00m)), Times.Once);
        }

        [Test]
        public async Task QueryMethods_ShouldReturnMappedResponses()
        {
           
            var pageRequest = new PageRequest { PageNumber = 1, PageSize = 10, SearchTerm = "test" };
            var list = new List<VendorSettlement> { new VendorSettlement { Id = 1 } };
            
            _mockSettlementRepo.Setup(r => r.GetSettlementsByVendorIdAsync(10)).ReturnsAsync(list);
            _mockSettlementRepo.Setup(r => r.GetPagedSettlementsWithDetails("test", 1, 10)).ReturnsAsync((list, 1));

            var expected = new List<VendorSettlementDTO> { new VendorSettlementDTO { Id = 1 } };
            _mockMapper.Setup(m => m.Map<ICollection<VendorSettlementDTO>>(list)).Returns(expected);
            _mockMapper.Setup(m => m.Map<List<VendorSettlementDTO>>(list)).Returns(expected);

            var settlements = await _settlementService.GetVendorSettlements(10);
            Assert.That(settlements.First().Id, Is.EqualTo(1));

            var paged = await _settlementService.GetOverallSettlements(pageRequest);
            Assert.That(paged.Items.First().Id, Is.EqualTo(1));
            Assert.That(paged.TotalCount, Is.EqualTo(1));
        }
    }
}

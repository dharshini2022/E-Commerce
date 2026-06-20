using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ecommerce.BLL;
using Ecommerce.Contracts.Repositories;
using Ecommerce.Models;
using Moq;
using NUnit.Framework;

namespace Ecommerce.Test
{
    [TestFixture]
    public class ShipmentServiceTest
    {
        private Mock<IShipmentRepository> _mockShipmentRepo;
        private ShipmentService _shipmentService;

        [SetUp]
        public void Setup()
        {
            _mockShipmentRepo = new Mock<IShipmentRepository>();
            _shipmentService = new ShipmentService(_mockShipmentRepo.Object);
        }

        [Test]
        public void GetShipmentDetails_ShouldThrowException_WhenNotFound()
        {
           
            _mockShipmentRepo.Setup(r => r.GetById(1)).ReturnsAsync((Shipment?)null);

            Assert.ThrowsAsync<KeyNotFoundException>(async () => await _shipmentService.GetShipmentDetails(1));
        }

        [Test]
        public async Task GetShipmentDetails_ShouldReturnShipment_WhenFound()
        {
           
            var shipment = new Shipment { Id = 1 };
            _mockShipmentRepo.Setup(r => r.GetById(1)).ReturnsAsync(shipment);

            
            var result = await _shipmentService.GetShipmentDetails(1);

            
            Assert.That(result, Is.EqualTo(shipment));
        }

        [Test]
        public async Task GetVendorShipments_ShouldReturnCollections()
        {
           
            var list = new List<Shipment> { new Shipment { Id = 1 } };
            _mockShipmentRepo.Setup(r => r.GetShipmentsByVendorIdAsync(10)).ReturnsAsync(list);

            
            var result = await _shipmentService.GetVendorShipments(10);

            
            Assert.That(result.First().Id, Is.EqualTo(1));
        }

        [Test]
        public async Task GetAllShipments_ShouldFilterByVendorId_WhenProvided()
        {
           
            var list = new List<Shipment> { new Shipment { Id = 1 } };
            _mockShipmentRepo.Setup(r => r.GetShipmentsByVendorIdAsync(10)).ReturnsAsync(list);

            
            var result = await _shipmentService.GetAllShipments(null, 10);

            
            Assert.That(result.First().Id, Is.EqualTo(1));
            _mockShipmentRepo.Verify(r => r.GetShipmentsByVendorIdAsync(10), Times.Once);
        }

        [Test]
        public async Task GetAllShipments_ShouldReturnAll_WhenVendorIdNotProvided()
        {
           
            var list = new List<Shipment> { new Shipment { Id = 1 } };
            _mockShipmentRepo.Setup(r => r.GetAll()).ReturnsAsync(list);

            
            var result = await _shipmentService.GetAllShipments(null, null);

            
            Assert.That(result.First().Id, Is.EqualTo(1));
            _mockShipmentRepo.Verify(r => r.GetAll(), Times.Once);
        }

        [Test]
        public async Task GetCustomerShipments_ShouldReturnCollections()
        {
           
            var list = new List<Shipment> { new Shipment { Id = 1 } };
            _mockShipmentRepo.Setup(r => r.GetShipmentsByUserIdAsync(100)).ReturnsAsync(list);

            
            var result = await _shipmentService.GetCustomerShipments(100);

            
            Assert.That(result.First().Id, Is.EqualTo(1));
        }

        [Test]
        public async Task ScheduleShipment_ShouldCreateShipment()
        {
           
            var shipment = new Shipment { Id = 5 };
            _mockShipmentRepo.Setup(r => r.Create(It.IsAny<Shipment>())).ReturnsAsync(shipment);

            
            var result = await _shipmentService.ScheduleShipment(1, 10, 15.00m);

            
            Assert.That(result.Id, Is.EqualTo(5));
            _mockShipmentRepo.Verify(r => r.Create(It.Is<Shipment>(s => s.UserAddressId == 10 && s.Status == ShipmentStatus.Pending && s.ShippingFee == 15.00m)), Times.Once);
        }

        [Test]
        public void ScheduleReturnPickup_ShouldThrowNotImplementedException()
        {
            Assert.ThrowsAsync<NotImplementedException>(async () => await _shipmentService.ScheduleReturnPickup(1, 2));
        }

        [Test]
        public void UpdateShipmentStatus_ShouldThrowException_WhenNotFound()
        {
           
            _mockShipmentRepo.Setup(r => r.GetById(1)).ReturnsAsync((Shipment?)null);

            Assert.ThrowsAsync<KeyNotFoundException>(async () => await _shipmentService.UpdateShipmentStatus(1, ShipmentStatus.Delivered));
        }

        [Test]
        public async Task UpdateShipmentStatus_ShouldUpdateStatusAndDates()
        {
           
            var shipment = new Shipment { Id = 1, Status = ShipmentStatus.Pending };
            _mockShipmentRepo.Setup(r => r.GetById(1)).ReturnsAsync(shipment);
            _mockShipmentRepo.Setup(r => r.Update(1, shipment)).ReturnsAsync(shipment);

            var result = await _shipmentService.UpdateShipmentStatus(1, ShipmentStatus.Initiated);
            Assert.That(result, Is.True);
            Assert.That(shipment.Status, Is.EqualTo(ShipmentStatus.Initiated));
            Assert.That(shipment.ShippedAt, Is.Not.Null);

            await _shipmentService.UpdateShipmentStatus(1, ShipmentStatus.Delivered);
            Assert.That(shipment.Status, Is.EqualTo(ShipmentStatus.Delivered));
            Assert.That(shipment.FulfilledAt, Is.Not.Null);
        }
    }
}

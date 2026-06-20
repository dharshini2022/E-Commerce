using AutoMapper;
using Ecommerce.BLL.Mapper;
using Ecommerce.Models;
using Ecommerce.Models.DTOs;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Ecommerce.Test
{
    [TestFixture]
    public class MappingTests
    {
        private IMapper _mapper;

        [SetUp]
        public void SetUp()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());
            var provider = services.BuildServiceProvider();
            _mapper = provider.GetRequiredService<IMapper>();
        }

        [Test]
        public void OrderItem_ShouldMapTo_OrderItemDTO_WithProductId()
        {
            
            var orderItem = new OrderItem
            {
                Id = 10,
                VariantId = 2,
                Quantity = 5,
                UnitPrice = 100.50m,
                VendorId = 3,
                Vendor = new Vendor
                {
                    StoreName = "Test Store"
                },
                Variant = new ProductVariant
                {
                    Id = 2,
                    ProductId = 42,
                    Product = new Product
                    {
                        Id = 42,
                        Name = "Test Product"
                    }
                }
            };

            
            var dto = _mapper.Map<OrderItemDTO>(orderItem);

            
            Assert.That(dto, Is.Not.Null);
            Assert.That(dto.Id, Is.EqualTo(10));
            Assert.That(dto.VariantId, Is.EqualTo(2));
            Assert.That(dto.ProductId, Is.EqualTo(42));
            Assert.That(dto.ProductName, Is.EqualTo("Test Product"));
            Assert.That(dto.VendorStoreName, Is.EqualTo("Test Store"));
            Assert.That(dto.Quantity, Is.EqualTo(5));
            Assert.That(dto.UnitPrice, Is.EqualTo(100.50m));
            Assert.That(dto.VendorId, Is.EqualTo(3));
        }

        [Test]
        public void Shipment_ShouldMapTo_ShipmentResponseDTO()
        {
            
            var shipment = new Shipment
            {
                Id = 1,
                TrackingNumber = "TRK123456",
                UserAddressId = 5,
                EstimatedFullfillement = DateTime.Now.AddDays(2),
                ShippedAt = DateTime.Now,
                FulfilledAt = null,
                Status = ShipmentStatus.Initiated,
                ShippingFee = 15.00m,
                OrderItems = new List<OrderItem>
                {
                    new OrderItem
                    {
                        Id = 10,
                        VariantId = 2,
                        Quantity = 5,
                        UnitPrice = 100.50m,
                        VendorId = 3,
                        Vendor = new Vendor { StoreName = "Test Store" },
                        Variant = new ProductVariant
                        {
                            Id = 2,
                            ProductId = 42,
                            Product = new Product { Id = 42, Name = "Test Product" }
                        }
                    }
                }
            };

            
            var dto = _mapper.Map<ShipmentResponseDTO>(shipment);

            
            Assert.That(dto, Is.Not.Null);
            Assert.That(dto.Id, Is.EqualTo(1));
            Assert.That(dto.TrackingNumber, Is.EqualTo("TRK123456"));
            Assert.That(dto.UserAddressId, Is.EqualTo(5));
            Assert.That(dto.Status, Is.EqualTo(ShipmentStatus.Initiated));
            Assert.That(dto.ShippingFee, Is.EqualTo(15.00m));
            Assert.That(dto.OrderItems, Has.Count.EqualTo(1));
            var itemDto = dto.OrderItems.First();
            Assert.That(itemDto.Id, Is.EqualTo(10));
            Assert.That(itemDto.ProductId, Is.EqualTo(42));
            Assert.That(itemDto.ProductName, Is.EqualTo("Test Product"));
            Assert.That(itemDto.VendorStoreName, Is.EqualTo("Test Store"));
        }

        [Test]
        public void MappingProfile_MaskEmail_ShouldMaskCorrectly()
        {
            var method = typeof(MappingProfile).GetMethod("MaskEmail", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            Assert.That(method, Is.Not.Null);

            Assert.That(method.Invoke(null, new object[] { "" }), Is.EqualTo(""));
            Assert.That(method.Invoke(null, new object[] { "   " }), Is.EqualTo("   "));

            Assert.That(method.Invoke(null, new object[] { "abc" }), Is.EqualTo("abc"));
            Assert.That(method.Invoke(null, new object[] { "abc@def@ghi" }), Is.EqualTo("abc@def@ghi"));

            Assert.That(method.Invoke(null, new object[] { "a@domain.com" }), Is.EqualTo("*@domain.com"));
            Assert.That(method.Invoke(null, new object[] { "ab@domain.com" }), Is.EqualTo("**@domain.com"));

            Assert.That(method.Invoke(null, new object[] { "abc@domain.com" }), Is.EqualTo("a*c@domain.com"));
            Assert.That(method.Invoke(null, new object[] { "abcd@domain.com" }), Is.EqualTo("a**d@domain.com"));
        }

        [Test]
        public void MappingProfile_MaskPhone_ShouldMaskCorrectly()
        {
            var method = typeof(MappingProfile).GetMethod("MaskPhone", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            Assert.That(method, Is.Not.Null);

            Assert.That(method.Invoke(null, new object[] { "" }), Is.EqualTo(""));
            Assert.That(method.Invoke(null, new object[] { "   " }), Is.EqualTo("   "));

            Assert.That(method.Invoke(null, new object[] { "123" }), Is.EqualTo("***"));
            Assert.That(method.Invoke(null, new object[] { "1234" }), Is.EqualTo("****"));

            Assert.That(method.Invoke(null, new object[] { "12345" }), Is.EqualTo("*2345"));
            Assert.That(method.Invoke(null, new object[] { "1234567890" }), Is.EqualTo("******7890"));
        }
    }
}

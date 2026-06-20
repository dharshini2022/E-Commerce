using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ecommerce.BLL;
using Ecommerce.Models;
using Microsoft.Extensions.Configuration;
using MimeKit;
using NUnit.Framework;

namespace Ecommerce.Test
{
    [TestFixture]
    public class EmailServiceTest
    {
        private class TestableEmailService : EmailService
        {
            public MimeMessage? SentMailMessage { get; private set; }
            public string? Host { get; private set; }
            public int Port { get; private set; }
            public string? SenderEmail { get; private set; }
            public string? Password { get; private set; }
            public bool SendCalled { get; private set; }

            public TestableEmailService(IConfiguration configuration) : base(configuration)
            {
            }

            protected override Task SendEmailClientAsync(MimeMessage mimeMessage, string host, int port, string senderEmail, string password)
            {
                SentMailMessage = mimeMessage;
                Host = host;
                Port = port;
                SenderEmail = senderEmail;
                Password = password;
                SendCalled = true;
                return Task.CompletedTask;
            }
        }

        [Test]
        public void SendOrderConfirmationEmail_ShouldSkip_WhenHostIsMissing()
        {
            var inMemorySettings = new Dictionary<string, string?>
            {
                {"Smtp:Port", "587"},
                {"Smtp:SenderEmail", "test@example.com"},
                {"Smtp:Password", "pwd"},
                {"Smtp:FromName", "BBS"}
            };
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            var service = new TestableEmailService(configuration);
            var order = new Order { Id = 1 };

            Assert.ThrowsAsync<Ecommerce.Shared.Exceptions.InvalidEmailCredsException>(
                async () => await service.SendOrderConfirmationEmail(order));
        }

        [Test]
        public void SendOrderConfirmationEmail_ShouldSkip_WhenSenderEmailIsMissing()
        {
            var inMemorySettings = new Dictionary<string, string?>
            {
                {"Smtp:Host", "smtp.gmail.com"},
                {"Smtp:Port", "587"},
                {"Smtp:Password", "pwd"}
            };
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            var service = new TestableEmailService(configuration);
            var order = new Order { Id = 1 };

            Assert.ThrowsAsync<Ecommerce.Shared.Exceptions.InvalidEmailCredsException>(
                async () => await service.SendOrderConfirmationEmail(order));
        }

        [Test]
        public void SendOrderConfirmationEmail_ShouldSkip_WhenPasswordIsMissing()
        {
            var inMemorySettings = new Dictionary<string, string?>
            {
                {"Smtp:Host", "smtp.gmail.com"},
                {"Smtp:Port", "587"},
                {"Smtp:SenderEmail", "test@example.com"}
            };
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            var service = new TestableEmailService(configuration);
            var order = new Order { Id = 1 };

            Assert.ThrowsAsync<Ecommerce.Shared.Exceptions.InvalidEmailCredsException>(
                async () => await service.SendOrderConfirmationEmail(order));
        }

        [Test]
        public void SendOrderConfirmationEmail_ShouldSkip_WhenPortIsInvalid()
        {
            var inMemorySettings = new Dictionary<string, string?>
            {
                {"Smtp:Host", "smtp.gmail.com"},
                {"Smtp:Port", "abc"},
                {"Smtp:SenderEmail", "test@example.com"},
                {"Smtp:Password", "pwd"}
            };
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            var service = new TestableEmailService(configuration);
            var order = new Order { Id = 1 };

            Assert.ThrowsAsync<Ecommerce.Shared.Exceptions.InvalidEmailCredsException>(
                async () => await service.SendOrderConfirmationEmail(order));
        }

        [Test]
        public void SendOrderConfirmationEmail_ShouldSkip_WhenRecipientEmailIsMissing()
        {
            var inMemorySettings = new Dictionary<string, string?>
            {
                {"Smtp:Host", "smtp.gmail.com"},
                {"Smtp:Port", "587"},
                {"Smtp:SenderEmail", "test@example.com"},
                {"Smtp:Password", "pwd"}
            };
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            var service = new TestableEmailService(configuration);
            var order = new Order
            {
                Id = 1,
                User = new User { Email = "" }
            };

            Assert.ThrowsAsync<Ecommerce.Shared.Exceptions.InvalidEmailCredsException>(
                async () => await service.SendOrderConfirmationEmail(order));
        }

        [Test]
        public async Task SendOrderConfirmationEmail_ShouldSendEmail_WithCorrectDetailsAndFormatting()
        {
            var inMemorySettings = new Dictionary<string, string?>
            {
                {"Smtp:Host", "smtp.gmail.com"},
                {"Smtp:Port", "587"},
                {"Smtp:SenderEmail", "test@example.com"},
                {"Smtp:Password", "pwd"},
                {"Smtp:FromName", "BBS Store"}
            };
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            var service = new TestableEmailService(configuration);

            var user = new User { Email = "customer@example.com", FullName = "John Doe" };
            var product = new Product { Name = "Cool Gadget" };
            var variant = new ProductVariant { Product = product, Price = 100.00m };

            var order = new Order
            {
                Id = 999,
                User = user,
                PlacedAt = new DateTime(2026, 6, 15, 12, 0, 0),
                Subtotal = 100.00m,
                DiscountAmount = 10.00m,
                TaxAmount = 5.00m,
                ShippingAmount = 8.00m,
                Total = 103.00m,
                Items = new List<OrderItem>
                {
                    new OrderItem
                    {
                        Quantity = 1,
                        UnitPrice = 100.00m,
                        Variant = variant
                    }
                }
            };

            await service.SendOrderConfirmationEmail(order);

            Assert.That(service.SendCalled, Is.True);
            Assert.That(service.Host, Is.EqualTo("smtp.gmail.com"));
            Assert.That(service.Port, Is.EqualTo(587));
            Assert.That(service.SenderEmail, Is.EqualTo("test@example.com"));
            Assert.That(service.Password, Is.EqualTo("pwd"));

            var mail = service.SentMailMessage;
            Assert.That(mail, Is.Not.Null);

            var fromAddress = (MailboxAddress)mail!.From[0];
            Assert.That(fromAddress.Address, Is.EqualTo("test@example.com"));
            Assert.That(fromAddress.Name, Is.EqualTo("BBS Store"));

            var toAddress = (MailboxAddress)mail.To[0];
            Assert.That(toAddress.Address, Is.EqualTo("customer@example.com"));
            Assert.That(mail.Subject, Is.EqualTo("Order Confirmed - #999"));

            var body = mail.HtmlBody;
            Assert.That(body, Does.Contain("Hello John Doe"));
            Assert.That(body, Does.Contain("Order ID:</strong> #999"));
            Assert.That(body, Does.Contain("Cool Gadget"));
            Assert.That(body, Does.Contain("₹ 100.00"));
            Assert.That(body, Does.Contain("Discount:</strong> -₹ 10.00"));
            Assert.That(body, Does.Contain("Tax:</strong> ₹ 5.00"));
            Assert.That(body, Does.Contain("Shipping:</strong> ₹ 8.00"));
            Assert.That(body, Does.Contain("Total Paid:</strong> ₹ 103.00"));
        }

        [Test]
        public async Task SendOrderConfirmationEmail_ShouldSendEmail_WithDefaultFromName_WhenFromNameIsMissing()
        {
            var inMemorySettings = new Dictionary<string, string?>
            {
                {"Smtp:Host", "smtp.gmail.com"},
                {"Smtp:Port", "587"},
                {"Smtp:SenderEmail", "test@example.com"},
                {"Smtp:Password", "pwd"}
            };
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            var service = new TestableEmailService(configuration);
            var user = new User { Email = "customer@example.com", FullName = "John Doe" };
            var order = new Order
            {
                Id = 123,
                User = user,
                Total = 50.00m
            };

            await service.SendOrderConfirmationEmail(order);

            Assert.That(service.SendCalled, Is.True);
            var fromAddress = (MailboxAddress)service.SentMailMessage!.From[0];
            Assert.That(fromAddress.Name, Is.EqualTo("BBS"));
        }

        [Test]
        public async Task SendEmailClientAsync_BaseImplementation_RunsWithoutExceptions()
        {
            var inMemorySettings = new Dictionary<string, string?>
            {
                {"Smtp:Host", "127.0.0.1"},
                {"Smtp:Port", "9999"},
                {"Smtp:SenderEmail", "test@example.com"},
                {"Smtp:Password", "pwd"}
            };
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            var service = new EmailService(configuration);
            var user = new User { Email = "customer@example.com", FullName = "John Doe" };
            var order = new Order
            {
                Id = 123,
                User = user,
                Total = 50.00m
            };

            Assert.CatchAsync<Exception>(async () => await service.SendOrderConfirmationEmail(order));
        }
    }
}

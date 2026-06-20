using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Ecommerce.BLL;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using Stripe;

namespace Ecommerce.Test
{
    [TestFixture]
    public class StripeServiceTest
    {
        private Mock<IConfiguration> _mockConfig;

        [SetUp]
        public void Setup()
        {
            _mockConfig = new Mock<IConfiguration>();
        }

        [Test]
        public void Constructor_ShouldThrowException_WhenSecretKeyMissing()
        {
           
            _mockConfig.Setup(c => c["Stripe:SecretKey"]).Returns((string?)null);

            Assert.Throws<InvalidOperationException>(() => new StripeService(_mockConfig.Object));
        }

        [Test]
        public async Task MakeStripeCharge_ShouldReturnResponse_WhenStripeSucceeds()
        {
           
            _mockConfig.Setup(c => c["Stripe:SecretKey"]).Returns("sk_test_mockkey");
            var stripeService = new StripeService(_mockConfig.Object);

            var mockClient = new Mock<IStripeClient>();
            var paymentIntent = new PaymentIntent 
            { 
                Id = "pi_123", 
                Status = "succeeded" 
            };

            mockClient.Setup(c => c.ApiKey).Returns("sk_test_mockkey");
            mockClient.Setup(c => c.RequestAsync<PaymentIntent>(
                It.IsAny<HttpMethod>(),
                It.IsAny<string>(),
                It.IsAny<BaseOptions>(),
                It.IsAny<RequestOptions>(),
                It.IsAny<CancellationToken>()
            )).ReturnsAsync(paymentIntent);

            var originalClient = StripeConfiguration.StripeClient;
            StripeConfiguration.StripeClient = mockClient.Object;

            try
            {
                
                var result = await stripeService.MakeStripeCharge(150.00m, "inr", "pm_123", "order_99");

                
                Assert.That(result, Is.Not.Null);
                Assert.That(result.PaymentIntentId, Is.EqualTo("pi_123"));
                Assert.That(result.Status, Is.EqualTo("succeeded"));
                Assert.That(result.Amount, Is.EqualTo(150.00m));
            }
            finally
            {
                StripeConfiguration.StripeClient = originalClient;
            }
        }
    }
}

using NUnit.Framework;
using Ecommerce.BLL;
using Ecommerce.Models;

namespace Ecommerce.Test
{
    [TestFixture]
    public class SummaryTests
    {
        [Test]
        public void OrderSummary_Properties_ShouldGetAndSet()
        {
            var summary = new OrderSummary
            {
                Subtotal = 100.50m,
                DiscountAmount = 10.00m,
                ShippingAmount = 5.00m,
                TaxAmount = 8.50m,
                PlatformCommission = 4.25m,
                Total = 108.25m
            };

            Assert.That(summary.Subtotal, Is.EqualTo(100.50m));
            Assert.That(summary.DiscountAmount, Is.EqualTo(10.00m));
            Assert.That(summary.ShippingAmount, Is.EqualTo(5.00m));
            Assert.That(summary.TaxAmount, Is.EqualTo(8.50m));
            Assert.That(summary.PlatformCommission, Is.EqualTo(4.25m));
            Assert.That(summary.Total, Is.EqualTo(108.25m));
        }

        [Test]
        public void VendorSummary_Properties_ShouldGetAndSet()
        {
            var summary = new VendorSummary
            {
                GrossAmount = 200.00m,
                ShippingAmount = 15.00m,
                VendorDiscountAmount = 20.00m,
                PlatformCommissionAmount = 10.00m,
                NetPayoutAmount = 185.00m,
                TransactionReference = "ref_123",
                SettlementStatus = SettlementStatus.Paid
            };

            Assert.That(summary.GrossAmount, Is.EqualTo(200.00m));
            Assert.That(summary.ShippingAmount, Is.EqualTo(15.00m));
            Assert.That(summary.VendorDiscountAmount, Is.EqualTo(20.00m));
            Assert.That(summary.PlatformCommissionAmount, Is.EqualTo(10.00m));
            Assert.That(summary.NetPayoutAmount, Is.EqualTo(185.00m));
            Assert.That(summary.TransactionReference, Is.EqualTo("ref_123"));
            Assert.That(summary.SettlementStatus, Is.EqualTo(SettlementStatus.Paid));
        }
    }
}

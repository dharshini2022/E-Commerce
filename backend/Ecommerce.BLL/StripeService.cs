using Ecommerce.Contracts.Services;
using Ecommerce.Models.DTOs;
using Microsoft.Extensions.Configuration;
using Stripe;

namespace Ecommerce.BLL
{
    public class StripeService : IStripeService
    {
        public StripeService(IConfiguration configuration)
        {
            var secretKey = configuration["Stripe:SecretKey"] ?? throw new InvalidOperationException("Stripe:SecretKey is not configured.");
            StripeConfiguration.ApiKey = secretKey;
        }

        public async Task<StripeChargeResponse> MakeStripeCharge(decimal amountInRupees, string currency, string paymentMethodId, string orderId)
        {
            var amountInPaise = (long)(amountInRupees * 100);

            var options = new PaymentIntentCreateOptions
            {
                Amount = amountInPaise,
                Currency = currency.ToLower(),
                PaymentMethod = paymentMethodId,
                Confirm = true,                         
                ReturnUrl = "https://example.com/payment-confirm",
                Metadata = new Dictionary<string, string>
                {
                    ["order_id"] = orderId,
                    ["platform"] = "Ecommerce"
                }
            };

            var stripePaymentService = new PaymentIntentService();
            var requestOptions = new RequestOptions
            {
                IdempotencyKey = $"payment-order-{orderId}"
            };
            var intent = await stripePaymentService.CreateAsync(options, requestOptions);

            return new StripeChargeResponse
            {
                PaymentIntentId = intent.Id,
                Status = intent.Status,  
                Amount = amountInRupees
            };
        }
    }
}

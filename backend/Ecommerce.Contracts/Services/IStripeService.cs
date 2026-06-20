using Ecommerce.Models;
using Ecommerce.Models.DTOs;
namespace Ecommerce.Contracts.Services
{
    public interface IStripeService
    {
        Task<StripeChargeResponse> MakeStripeCharge(decimal amountInRupees, string currency, string paymentMethodId, string orderId);
    }
   
}

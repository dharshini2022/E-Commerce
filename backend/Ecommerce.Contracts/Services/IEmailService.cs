using System.Threading.Tasks;
using Ecommerce.Models;

namespace Ecommerce.Contracts.Services
{
    public interface IEmailService
    {
        Task SendOrderConfirmationEmail(Order order);
    }
}

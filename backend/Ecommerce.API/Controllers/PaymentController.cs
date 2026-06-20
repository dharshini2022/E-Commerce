using Ecommerce.Contracts.Services;
using Ecommerce.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpPost("pay/{orderId}")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> MakePayment([FromRoute] int orderId, [FromBody] MakePaymentRequest request)
        {
            var result = await _paymentService.MakePayment(orderId, request);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("history")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetMyPaymentHistory()
        {
            var result = await _paymentService.GetMyPaymentHistory();
            return Ok(result);
        }

        [HttpGet("overall")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetOverallPaymentHistory([FromQuery] PageRequest request)
        {
            var result = await _paymentService.GetOverallPaymentHistory(request);
            return Ok(result);
        }
    }
}

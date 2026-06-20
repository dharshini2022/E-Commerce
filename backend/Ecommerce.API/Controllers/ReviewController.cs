using Ecommerce.Contracts.Services;
using Ecommerce.Contracts.Repositories;
using Ecommerce.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewService _reviewService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IVendorRepository _vendorRepository;

        public ReviewController(
            IReviewService reviewService,
            ICurrentUserService currentUserService,
            IVendorRepository vendorRepository)
        {
            _reviewService = reviewService;
            _currentUserService = currentUserService;
            _vendorRepository = vendorRepository;
        }

        [HttpPost]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> PostReview([FromBody] CreateReviewRequest request)
        {
            var userId = _currentUserService.UserId;
            var result = await _reviewService.AddReview(userId, request);
            return CreatedAtAction(nameof(GetProductReviews), new { productId = result.ProductId }, result);
        }

        [HttpGet("my")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetMyReviews()
        {
            var userId = _currentUserService.UserId;
            var result = await _reviewService.GetUserReviews(userId);
            return Ok(result);
        }

        [HttpPut("{reviewId}")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> PutReview([FromRoute] int reviewId, [FromBody] UpdateReviewRequest request)
        {
            var userId = _currentUserService.UserId;
            var result = await _reviewService.UpdateReview(userId, reviewId, request);
            return Ok(result);
        }

        [HttpDelete("{reviewId}")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> DeleteReview([FromRoute] int reviewId)
        {
            var userId = _currentUserService.UserId;
            var result = await _reviewService.DeleteReview(userId, reviewId);
            return Ok(new { success = result, message = "Review deleted successfully." });
        }

        [HttpGet("product/{productId}")]
        [Authorize(Roles = "Customer,Vendor,Admin")]
        public async Task<IActionResult> GetProductReviews([FromRoute] int productId)
        {
            var result = await _reviewService.GetProductReviews(productId);
            return Ok(result);
        }

        [HttpGet("overall")]
        [Authorize(Roles = "Vendor,Admin")]
        public async Task<IActionResult> GetOverallReviews()
        {
            if (User.IsInRole("Admin"))
            {
                var result = await _reviewService.GetAllReviews();
                return Ok(result);
            }
            else if (User.IsInRole("Vendor"))
            {
                var userId = _currentUserService.UserId;
                var vendor = await _vendorRepository.GetByUserId(userId);
                if (vendor == null)
                {
                    return NotFound(new { message = "Vendor profile not found." });
                }
                var result = await _reviewService.GetVendorReviews(vendor.Id);
                return Ok(result);
            }

            return Forbid();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Ecommerce.Contracts.Repositories;
using Ecommerce.Contracts.Services;
using Ecommerce.Models;
using Ecommerce.Models.DTOs;

namespace Ecommerce.BLL
{
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly IReviewImageRepository _reviewImageRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IVendorRepository _vendorRepository;
        private readonly IMapper _mapper;

        public ReviewService(IReviewRepository reviewRepository,IReviewImageRepository reviewImageRepository,IOrderRepository orderRepository,IVendorRepository vendorRepository,IMapper mapper)
        {
            _reviewRepository = reviewRepository;
            _reviewImageRepository = reviewImageRepository;
            _orderRepository = orderRepository;
            _vendorRepository = vendorRepository;
            _mapper = mapper;
        }

        public async Task<ReviewDTO> AddReview(int userId, CreateReviewRequest request)
        {
            var order = await _orderRepository.GetOrderWithDetailsById(request.OrderId);
            if (order == null || order.UserId != userId)
            {
                throw new KeyNotFoundException("Order not found or does not belong to the user.");
            }

            bool hasProduct = order.Items.Any(item => item.Variant.ProductId == request.ProductId);
            if (!hasProduct)
            {
                throw new InvalidOperationException("The order does not contain the specified product.");
            }

            if (order.OrderPaymentStatus != PaymentStatus.Paid)
            {
                throw new InvalidOperationException("You can only review products from paid orders.");
            }

            var existingReviews = await _reviewRepository.GetAll();
            var alreadyReviewed = existingReviews.Any(r => r.OrderId == request.OrderId && r.ProductId == request.ProductId);
            if (alreadyReviewed)
            {
                throw new InvalidOperationException("You have already reviewed this product for this order.");
            }

            if (request.Rating < 0m || request.Rating > 5m)
            {
                throw new ArgumentException("Rating must be between 0.0 and 5.0.");
            }

            var review = new Review
            {
                ProductId = request.ProductId,
                UserId = userId,
                OrderId = request.OrderId,
                Rating = request.Rating,
                Title = request.Title,
                Body = request.Body,
                CreatedAt = DateTime.Now,
                ReviewImages = request.ImageUrls.Select((url, index) => new ReviewImage
                {
                    ImageUrl = url,
                    ImageOrder = index
                }).ToList()
            };

            var createdReview = await _reviewRepository.Create(review);
            var reviewWithDetails = await _reviewRepository.GetReviewWithDetailsByIdAsync(createdReview.Id);
            return _mapper.Map<ReviewDTO>(reviewWithDetails);
        }

        public async Task<ReviewDTO> UpdateReview(int userId, int reviewId, UpdateReviewRequest request)
        {
            var review = await _reviewRepository.GetReviewWithDetailsByIdAsync(reviewId);
            if (review == null)
            {
                throw new KeyNotFoundException("Review not found.");
            }

            if (review.UserId != userId)
            {
                throw new UnauthorizedAccessException("You are not authorized to update this review.");
            }

            if (request.Rating < 0m || request.Rating > 5m)
            {
                throw new ArgumentException("Rating must be between 0.0 and 5.0.");
            }

            review.Rating = request.Rating;
            review.Title = request.Title;
            review.Body = request.Body;

            await _reviewImageRepository.HardDeleteImagesByReviewId(reviewId);

            if (request.ImageUrls != null && request.ImageUrls.Any())
            {
                foreach (var (url, index) in request.ImageUrls.Select((url, index) => (url, index)))
                {
                    await _reviewImageRepository.Create(new ReviewImage
                    {
                        ReviewId = reviewId,
                        ImageUrl = url,
                        ImageOrder = index
                    });
                }
            }

            await _reviewRepository.Update(review.Id, review);

            var updatedReview = await _reviewRepository.GetReviewWithDetailsByIdAsync(reviewId);
            return _mapper.Map<ReviewDTO>(updatedReview);
        }

        public async Task<bool> DeleteReview(int userId, int reviewId)
        {
            var review = await _reviewRepository.GetById(reviewId);
            if (review == null)
            {
                throw new KeyNotFoundException("Review not found.");
            }

            if (review.UserId != userId)
            {
                throw new UnauthorizedAccessException("You are not authorized to delete this review.");
            }

            await _reviewImageRepository.HardDeleteImagesByReviewId(reviewId);
            await _reviewRepository.Delete(reviewId);
            return true;
        }

        public async Task<ICollection<ReviewDTO>> GetUserReviews(int userId)
        {
            var reviews = await _reviewRepository.GetReviewsByUserIdAsync(userId);
            return _mapper.Map<ICollection<ReviewDTO>>(reviews);
        }

        public async Task<ICollection<ReviewDTO>> GetProductReviews(int productId)
        {
            var reviews = await _reviewRepository.GetReviewsByProductIdAsync(productId);
            return _mapper.Map<ICollection<ReviewDTO>>(reviews);
        }

        public async Task<ICollection<ReviewDTO>> GetVendorReviews(int vendorId)
        {
            var reviews = await _reviewRepository.GetReviewsByVendorIdAsync(vendorId);
            return _mapper.Map<ICollection<ReviewDTO>>(reviews);
        }

        public async Task<ICollection<ReviewDTO>> GetAllReviews()
        {
            var reviews = await _reviewRepository.GetAllReviewsWithDetails();
            return _mapper.Map<ICollection<ReviewDTO>>(reviews);
        }
    }
}

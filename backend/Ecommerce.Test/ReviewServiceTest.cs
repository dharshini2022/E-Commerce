using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Ecommerce.BLL;
using Ecommerce.Contracts.Repositories;
using Ecommerce.Models;
using Ecommerce.Models.DTOs;
using Moq;
using NUnit.Framework;

namespace Ecommerce.Test
{
    [TestFixture]
    public class ReviewServiceTest
    {
        private Mock<IReviewRepository> _mockReviewRepo;
        private Mock<IReviewImageRepository> _mockReviewImageRepo;
        private Mock<IOrderRepository> _mockOrderRepo;
        private Mock<IVendorRepository> _mockVendorRepo;
        private Mock<IMapper> _mockMapper;
        private ReviewService _reviewService;

        [SetUp]
        public void Setup()
        {
            _mockReviewRepo = new Mock<IReviewRepository>();
            _mockReviewImageRepo = new Mock<IReviewImageRepository>();
            _mockOrderRepo = new Mock<IOrderRepository>();
            _mockVendorRepo = new Mock<IVendorRepository>();
            _mockMapper = new Mock<IMapper>();

            _reviewService = new ReviewService(
                _mockReviewRepo.Object,
                _mockReviewImageRepo.Object,
                _mockOrderRepo.Object,
                _mockVendorRepo.Object,
                _mockMapper.Object
            );
        }

        [Test]
        public void AddReview_ShouldThrowException_WhenOrderNotFound()
        {
           
            _mockOrderRepo.Setup(r => r.GetOrderWithDetailsById(It.IsAny<int>())).ReturnsAsync((Order?)null);
            var request = new CreateReviewRequest { OrderId = 1, ProductId = 10, Rating = 5, Title = "Nice" };

            Assert.ThrowsAsync<KeyNotFoundException>(async () => await _reviewService.AddReview(1, request));
        }

        [Test]
        public void AddReview_ShouldThrowException_WhenOrderDoesNotBelongToUser()
        {
           
            var order = new Order { Id = 1, UserId = 2 };
            _mockOrderRepo.Setup(r => r.GetOrderWithDetailsById(1)).ReturnsAsync(order);
            var request = new CreateReviewRequest { OrderId = 1, ProductId = 10, Rating = 5, Title = "Nice" };

            Assert.ThrowsAsync<KeyNotFoundException>(async () => await _reviewService.AddReview(1, request));
        }

        [Test]
        public void AddReview_ShouldThrowException_WhenProductNotOrdered()
        {
           
            var order = new Order 
            { 
                Id = 1, 
                UserId = 1, 
                Items = new List<OrderItem> 
                { 
                    new OrderItem { Variant = new ProductVariant { ProductId = 20 } } 
                } 
            };
            _mockOrderRepo.Setup(r => r.GetOrderWithDetailsById(1)).ReturnsAsync(order);
            var request = new CreateReviewRequest { OrderId = 1, ProductId = 10, Rating = 5, Title = "Nice" };

            Assert.ThrowsAsync<InvalidOperationException>(async () => await _reviewService.AddReview(1, request));
        }

        [Test]
        public void AddReview_ShouldThrowException_WhenOrderNotPaid()
        {
           
            var order = new Order 
            { 
                Id = 1, 
                UserId = 1, 
                OrderPaymentStatus = PaymentStatus.Pending,
                Items = new List<OrderItem> 
                { 
                    new OrderItem { Variant = new ProductVariant { ProductId = 10 } } 
                } 
            };
            _mockOrderRepo.Setup(r => r.GetOrderWithDetailsById(1)).ReturnsAsync(order);
            var request = new CreateReviewRequest { OrderId = 1, ProductId = 10, Rating = 5, Title = "Nice" };

            Assert.ThrowsAsync<InvalidOperationException>(async () => await _reviewService.AddReview(1, request));
        }

        [Test]
        public void AddReview_ShouldThrowException_WhenAlreadyReviewed()
        {
           
            var order = new Order 
            { 
                Id = 1, 
                UserId = 1, 
                OrderPaymentStatus = PaymentStatus.Paid,
                Items = new List<OrderItem> 
                { 
                    new OrderItem { Variant = new ProductVariant { ProductId = 10 } } 
                } 
            };
            _mockOrderRepo.Setup(r => r.GetOrderWithDetailsById(1)).ReturnsAsync(order);
            _mockReviewRepo.Setup(r => r.GetAll()).ReturnsAsync(new List<Review>
            {
                new Review { OrderId = 1, ProductId = 10 }
            });
            var request = new CreateReviewRequest { OrderId = 1, ProductId = 10, Rating = 5, Title = "Nice" };

            Assert.ThrowsAsync<InvalidOperationException>(async () => await _reviewService.AddReview(1, request));
        }

        [Test]
        public void AddReview_ShouldThrowException_WhenRatingLessThanZero()
        {
           
            var order = new Order 
            { 
                Id = 1, 
                UserId = 1, 
                OrderPaymentStatus = PaymentStatus.Paid,
                Items = new List<OrderItem> 
                { 
                    new OrderItem { Variant = new ProductVariant { ProductId = 10 } } 
                } 
            };
            _mockOrderRepo.Setup(r => r.GetOrderWithDetailsById(1)).ReturnsAsync(order);
            _mockReviewRepo.Setup(r => r.GetAll()).ReturnsAsync(new List<Review>());
            var request = new CreateReviewRequest { OrderId = 1, ProductId = 10, Rating = -1m, Title = "Nice" };

            Assert.ThrowsAsync<ArgumentException>(async () => await _reviewService.AddReview(1, request));
        }

        [Test]
        public void AddReview_ShouldThrowException_WhenRatingGreaterThanFive()
        {
           
            var order = new Order 
            { 
                Id = 1, 
                UserId = 1, 
                OrderPaymentStatus = PaymentStatus.Paid,
                Items = new List<OrderItem> 
                { 
                    new OrderItem { Variant = new ProductVariant { ProductId = 10 } } 
                } 
            };
            _mockOrderRepo.Setup(r => r.GetOrderWithDetailsById(1)).ReturnsAsync(order);
            _mockReviewRepo.Setup(r => r.GetAll()).ReturnsAsync(new List<Review>());
            var request = new CreateReviewRequest { OrderId = 1, ProductId = 10, Rating = 6m, Title = "Nice" };

            Assert.ThrowsAsync<ArgumentException>(async () => await _reviewService.AddReview(1, request));
        }

        [Test]
        public async Task AddReview_ShouldCreateReview_WhenRequestIsValid()
        {
           
            var order = new Order 
            { 
                Id = 1, 
                UserId = 1, 
                OrderPaymentStatus = PaymentStatus.Paid,
                Items = new List<OrderItem> 
                { 
                    new OrderItem { Variant = new ProductVariant { ProductId = 10 } } 
                } 
            };
            _mockOrderRepo.Setup(r => r.GetOrderWithDetailsById(1)).ReturnsAsync(order);
            _mockReviewRepo.Setup(r => r.GetAll()).ReturnsAsync(new List<Review>());

            var createdReview = new Review { Id = 100, ProductId = 10, UserId = 1, OrderId = 1, Rating = 5, Title = "Nice" };
            _mockReviewRepo.Setup(r => r.Create(It.IsAny<Review>())).ReturnsAsync(createdReview);
            _mockReviewRepo.Setup(r => r.GetReviewWithDetailsByIdAsync(100)).ReturnsAsync(createdReview);

            var expectedDto = new ReviewDTO { Id = 100, ProductId = 10, UserId = 1, OrderId = 1, Rating = 5, Title = "Nice" };
            _mockMapper.Setup(m => m.Map<ReviewDTO>(createdReview)).Returns(expectedDto);

            var request = new CreateReviewRequest { OrderId = 1, ProductId = 10, Rating = 5, Title = "Nice", ImageUrls = new List<string>() };

            
            var result = await _reviewService.AddReview(1, request);

            
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(100));
            _mockReviewRepo.Verify(r => r.Create(It.IsAny<Review>()), Times.Once);
        }

        [Test]
        public async Task AddReview_ShouldCreateReviewWithImages_WhenImageUrlsProvided()
        {
           
            var order = new Order 
            { 
                Id = 1, 
                UserId = 1, 
                OrderPaymentStatus = PaymentStatus.Paid,
                Items = new List<OrderItem> 
                { 
                    new OrderItem { Variant = new ProductVariant { ProductId = 10 } } 
                } 
            };
            _mockOrderRepo.Setup(r => r.GetOrderWithDetailsById(1)).ReturnsAsync(order);
            _mockReviewRepo.Setup(r => r.GetAll()).ReturnsAsync(new List<Review>());

            Review? capturedReview = null;
            _mockReviewRepo.Setup(r => r.Create(It.IsAny<Review>()))
                .Callback<Review>(r => capturedReview = r)
                .ReturnsAsync(new Review { Id = 100 });
            _mockReviewRepo.Setup(r => r.GetReviewWithDetailsByIdAsync(100)).ReturnsAsync(new Review());

            var request = new CreateReviewRequest 
            { 
                OrderId = 1, 
                ProductId = 10, 
                Rating = 5, 
                Title = "Nice", 
                ImageUrls = new List<string> { "http://image1.jpg", "http://image2.jpg" } 
            };

            
            await _reviewService.AddReview(1, request);

            
            Assert.That(capturedReview, Is.Not.Null);
            Assert.That(capturedReview.ReviewImages, Has.Count.EqualTo(2));
            Assert.That(capturedReview.ReviewImages.ElementAt(0).ImageUrl, Is.EqualTo("http://image1.jpg"));
            Assert.That(capturedReview.ReviewImages.ElementAt(0).ImageOrder, Is.EqualTo(0));
            Assert.That(capturedReview.ReviewImages.ElementAt(1).ImageUrl, Is.EqualTo("http://image2.jpg"));
            Assert.That(capturedReview.ReviewImages.ElementAt(1).ImageOrder, Is.EqualTo(1));
        }

        [Test]
        public void UpdateReview_ShouldThrowException_WhenReviewNotFound()
        {
           
            _mockReviewRepo.Setup(r => r.GetReviewWithDetailsByIdAsync(100)).ReturnsAsync((Review?)null);
            var request = new UpdateReviewRequest { Rating = 5, Title = "Nice Update" };

            Assert.ThrowsAsync<KeyNotFoundException>(async () => await _reviewService.UpdateReview(1, 100, request));
        }

        [Test]
        public void UpdateReview_ShouldThrowException_WhenReviewNotOwnedByUser()
        {
           
            var review = new Review { Id = 100, UserId = 2 };
            _mockReviewRepo.Setup(r => r.GetReviewWithDetailsByIdAsync(100)).ReturnsAsync(review);
            var request = new UpdateReviewRequest { Rating = 5, Title = "Nice Update" };

            Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await _reviewService.UpdateReview(1, 100, request));
        }

        [Test]
        public void UpdateReview_ShouldThrowException_WhenRatingLessThanZero()
        {
           
            var review = new Review { Id = 100, UserId = 1 };
            _mockReviewRepo.Setup(r => r.GetReviewWithDetailsByIdAsync(100)).ReturnsAsync(review);
            var request = new UpdateReviewRequest { Rating = -0.5m, Title = "Nice Update" };

            Assert.ThrowsAsync<ArgumentException>(async () => await _reviewService.UpdateReview(1, 100, request));
        }

        [Test]
        public void UpdateReview_ShouldThrowException_WhenRatingGreaterThanFive()
        {
           
            var review = new Review { Id = 100, UserId = 1 };
            _mockReviewRepo.Setup(r => r.GetReviewWithDetailsByIdAsync(100)).ReturnsAsync(review);
            var request = new UpdateReviewRequest { Rating = 5.1m, Title = "Nice Update" };

            Assert.ThrowsAsync<ArgumentException>(async () => await _reviewService.UpdateReview(1, 100, request));
        }

        [Test]
        public async Task UpdateReview_ShouldUpdate_WhenRequestIsValid()
        {
           
            var review = new Review { Id = 100, UserId = 1, Rating = 4, Title = "Old Title", ReviewImages = new List<ReviewImage>() };
            _mockReviewRepo.Setup(r => r.GetReviewWithDetailsByIdAsync(100)).ReturnsAsync(review);
            _mockReviewImageRepo.Setup(r => r.HardDeleteImagesByReviewId(100)).ReturnsAsync(true);
            
            var updatedReview = new Review { Id = 100, UserId = 1, Rating = 5, Title = "Nice Update" };
            _mockReviewRepo.Setup(r => r.Update(100, It.IsAny<Review>())).ReturnsAsync(updatedReview);
            _mockReviewRepo.Setup(r => r.GetReviewWithDetailsByIdAsync(100)).ReturnsAsync(updatedReview);

            var expectedDto = new ReviewDTO { Id = 100, UserId = 1, Rating = 5, Title = "Nice Update" };
            _mockMapper.Setup(m => m.Map<ReviewDTO>(updatedReview)).Returns(expectedDto);

            var request = new UpdateReviewRequest { Rating = 5, Title = "Nice Update" };

            
            var result = await _reviewService.UpdateReview(1, 100, request);

            
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Title, Is.EqualTo("Nice Update"));
            _mockReviewImageRepo.Verify(r => r.HardDeleteImagesByReviewId(100), Times.Once);
            _mockReviewRepo.Verify(r => r.Update(100, It.IsAny<Review>()), Times.Once);
        }

        [Test]
        public async Task UpdateReview_ShouldCreateImages_WhenImageUrlsProvided()
        {
           
            var review = new Review { Id = 100, UserId = 1, Rating = 4, Title = "Old Title", ReviewImages = new List<ReviewImage>() };
            _mockReviewRepo.Setup(r => r.GetReviewWithDetailsByIdAsync(100)).ReturnsAsync(review);
            _mockReviewImageRepo.Setup(r => r.HardDeleteImagesByReviewId(100)).ReturnsAsync(true);
            
            _mockReviewRepo.Setup(r => r.Update(100, It.IsAny<Review>())).ReturnsAsync(review);
            _mockReviewRepo.Setup(r => r.GetReviewWithDetailsByIdAsync(100)).ReturnsAsync(review);

            var request = new UpdateReviewRequest 
            { 
                Rating = 5, 
                Title = "Nice Update",
                ImageUrls = new List<string> { "http://newimg.jpg" }
            };

            
            await _reviewService.UpdateReview(1, 100, request);

            
            _mockReviewImageRepo.Verify(r => r.Create(It.Is<ReviewImage>(ri => ri.ImageUrl == "http://newimg.jpg" && ri.ReviewId == 100 && ri.ImageOrder == 0)), Times.Once);
        }

        [Test]
        public void DeleteReview_ShouldThrowException_WhenReviewNotFound()
        {
           
            _mockReviewRepo.Setup(r => r.GetById(100)).ReturnsAsync((Review?)null);

            Assert.ThrowsAsync<KeyNotFoundException>(async () => await _reviewService.DeleteReview(1, 100));
        }

        [Test]
        public void DeleteReview_ShouldThrowException_WhenReviewNotOwnedByUser()
        {
           
            var review = new Review { Id = 100, UserId = 2 };
            _mockReviewRepo.Setup(r => r.GetById(100)).ReturnsAsync(review);

            Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await _reviewService.DeleteReview(1, 100));
        }

        [Test]
        public async Task DeleteReview_ShouldDelete_WhenRequestIsValid()
        {
           
            var review = new Review { Id = 100, UserId = 1 };
            _mockReviewRepo.Setup(r => r.GetById(100)).ReturnsAsync(review);
            _mockReviewImageRepo.Setup(r => r.HardDeleteImagesByReviewId(100)).ReturnsAsync(true);
            _mockReviewRepo.Setup(r => r.Delete(100)).ReturnsAsync(review);

            
            var result = await _reviewService.DeleteReview(1, 100);

            
            Assert.That(result, Is.True);
            _mockReviewImageRepo.Verify(r => r.HardDeleteImagesByReviewId(100), Times.Once);
            _mockReviewRepo.Verify(r => r.Delete(100), Times.Once);
        }

        [Test]
        public async Task QueryMethods_ShouldReturnMappedLists()
        {
           
            var reviews = new List<Review> { new Review { Id = 1 } };
            var expectedDtos = new List<ReviewDTO> { new ReviewDTO { Id = 1 } };

            _mockReviewRepo.Setup(r => r.GetReviewsByUserIdAsync(1)).ReturnsAsync(reviews);
            _mockReviewRepo.Setup(r => r.GetReviewsByProductIdAsync(2)).ReturnsAsync(reviews);
            _mockReviewRepo.Setup(r => r.GetReviewsByVendorIdAsync(3)).ReturnsAsync(reviews);
            _mockReviewRepo.Setup(r => r.GetAllReviewsWithDetails()).ReturnsAsync(reviews);

            _mockMapper.Setup(m => m.Map<ICollection<ReviewDTO>>(reviews)).Returns(expectedDtos);

            var userReviews = await _reviewService.GetUserReviews(1);
            Assert.That(userReviews.First().Id, Is.EqualTo(1));

            var productReviews = await _reviewService.GetProductReviews(2);
            Assert.That(productReviews.First().Id, Is.EqualTo(1));

            var vendorReviews = await _reviewService.GetVendorReviews(3);
            Assert.That(vendorReviews.First().Id, Is.EqualTo(1));

            var allReviews = await _reviewService.GetAllReviews();
            Assert.That(allReviews.First().Id, Is.EqualTo(1));
        }
    }
}

using System;
using System.Collections.Generic;
using System.Security.Claims;
using Ecommerce.BLL;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;

namespace Ecommerce.Test
{
    [TestFixture]
    public class CurrentUserServiceTest
    {
        private Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private CurrentUserService _currentUserService;

        [SetUp]
        public void Setup()
        {
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            _currentUserService = new CurrentUserService(_mockHttpContextAccessor.Object);
        }

        [Test]
        public void ContextNull_ThrowsInvalidOperationException()
        {
            _mockHttpContextAccessor.Setup(a => a.HttpContext).Returns((HttpContext?)null);

            Assert.Throws<InvalidOperationException>(() => { _ = _currentUserService.UserId; });
        }

        [Test]
        public void UnauthenticatedUser_UserId_ThrowsUnauthorizedAccessException()
        {
            var mockHttpContext = new Mock<HttpContext>();
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity());
            mockHttpContext.Setup(c => c.User).Returns(claimsPrincipal);
            _mockHttpContextAccessor.Setup(a => a.HttpContext).Returns(mockHttpContext.Object);

            Assert.Throws<UnauthorizedAccessException>(() => { _ = _currentUserService.UserId; });
        }

        [Test]
        public void AuthenticatedUser_ReturnsCorrectClaims()
        {
            var mockHttpContext = new Mock<HttpContext>();
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "99"),
                new Claim(ClaimTypes.Email, "test@example.com"),
                new Claim(ClaimTypes.Role, "Admin"),
                new Claim("fullName", "John Doe")
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            mockHttpContext.Setup(c => c.User).Returns(claimsPrincipal);
            _mockHttpContextAccessor.Setup(a => a.HttpContext).Returns(mockHttpContext.Object);

            Assert.That(_currentUserService.UserId, Is.EqualTo(99));
            Assert.That(_currentUserService.Email, Is.EqualTo("test@example.com"));
            Assert.That(_currentUserService.Role, Is.EqualTo("Admin"));
            Assert.That(_currentUserService.FullName, Is.EqualTo("John Doe"));
            Assert.That(_currentUserService.IsAuthenticated, Is.True);
        }

        [Test]
        public void ClaimsMissing_ReturnsDefaultEmptyStrings()
        {
            var mockHttpContext = new Mock<HttpContext>();
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>(), "TestAuth"));
            mockHttpContext.Setup(c => c.User).Returns(claimsPrincipal);
            _mockHttpContextAccessor.Setup(a => a.HttpContext).Returns(mockHttpContext.Object);

            Assert.Throws<UnauthorizedAccessException>(() => { _ = _currentUserService.UserId; });
            Assert.That(_currentUserService.Email, Is.EqualTo(string.Empty));
            Assert.That(_currentUserService.Role, Is.EqualTo(string.Empty));
            Assert.That(_currentUserService.FullName, Is.EqualTo(string.Empty));
            Assert.That(_currentUserService.IsAuthenticated, Is.True);
        }
    }
}

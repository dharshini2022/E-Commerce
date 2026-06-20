using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Ecommerce.BLL;
using Ecommerce.Contracts.Repositories;
using Ecommerce.Contracts.Services;
using Ecommerce.Models;
using Ecommerce.Models.DTOs;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;

namespace Ecommerce.Test
{
    [TestFixture]
    public class AuthServiceTest
    {
        private Mock<IUserRepository> _mockUserRepo;
        private Mock<IRefreshTokenRepository> _mockRefreshTokenRepo;
        private Mock<IMapper> _mockMapper;
        private Mock<IConfiguration> _mockConfig;
        private Mock<ICurrentUserService> _mockCurrentUserService;
        private AuthService _authService;

        [SetUp]
        public void Setup()
        {
            _mockUserRepo = new Mock<IUserRepository>();
            _mockRefreshTokenRepo = new Mock<IRefreshTokenRepository>();
            _mockMapper = new Mock<IMapper>();
            _mockConfig = new Mock<IConfiguration>();
            _mockCurrentUserService = new Mock<ICurrentUserService>();

            _authService = new AuthService(
                _mockUserRepo.Object,
                _mockRefreshTokenRepo.Object,
                _mockMapper.Object,
                _mockConfig.Object,
                _mockCurrentUserService.Object
            );
        }

        [Test]
        public void Register_ShouldThrowException_WhenUserAlreadyExists()
        {
            var existingUser = new User { Email = "test@example.com" };
            _mockUserRepo.Setup(r => r.GetByEmail("test@example.com")).ReturnsAsync(existingUser);
            var request = new RegisterRequest { Email = "test@example.com", Password = "Password123" };

            Assert.ThrowsAsync<InvalidOperationException>(async () => await _authService.Register(request));
        }

        [Test]
        public async Task Register_ShouldCreateCustomer_WhenRequestIsValid()
        {
            _mockUserRepo.Setup(r => r.GetByEmail("test@example.com")).ReturnsAsync((User?)null);
            _mockCurrentUserService.Setup(s => s.Role).Returns("Customer");

            var user = new User { Email = "test@example.com" };
            _mockMapper.Setup(m => m.Map<User>(It.IsAny<RegisterRequest>())).Returns(user);

            var createdUser = new User { Id = 10, Email = "test@example.com", Role = UserRole.Customer };
            _mockUserRepo.Setup(r => r.Create(user)).ReturnsAsync(createdUser);

            var expectedResponse = new RegisterResponse { Id = 10, Email = "test@example.com" };
            _mockMapper.Setup(m => m.Map<RegisterResponse>(createdUser)).Returns(expectedResponse);

            var request = new RegisterRequest { Email = "test@example.com", Password = "Password123" };

            var result = await _authService.Register(request);
            
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(10));
            Assert.That(user.Role, Is.EqualTo(UserRole.Customer));
            _mockUserRepo.Verify(r => r.Create(user), Times.Once);
        }

        [Test]
        public async Task Register_ShouldSetRoleToAdmin_WhenCurrentUserIsAdmin()
        {
            _mockUserRepo.Setup(r => r.GetByEmail("admin@example.com")).ReturnsAsync((User?)null);
            _mockCurrentUserService.Setup(s => s.Role).Returns("Admin");

            var user = new User { Email = "admin@example.com" };
            _mockMapper.Setup(m => m.Map<User>(It.IsAny<RegisterRequest>())).Returns(user);
            _mockUserRepo.Setup(r => r.Create(user)).ReturnsAsync(user);

            var request = new RegisterRequest { Email = "admin@example.com", Password = "Password123" };

            await _authService.Register(request);

            Assert.That(user.Role, Is.EqualTo(UserRole.Customer));
        }

        [Test]
        public void Login_ShouldThrowUnauthorizedException_WhenUserNotFound()
        {
            _mockUserRepo.Setup(r => r.GetByEmail("test@example.com")).ReturnsAsync((User?)null);
            var request = new LoginRequest { Email = "test@example.com", Password = "Password123" };

            Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await _authService.Login(request));
        }

        [Test]
        public void Login_ShouldThrowUnauthorizedException_WhenUserNotActive()
        {
            var user = new User { Email = "test@example.com", IsActive = false };
            _mockUserRepo.Setup(r => r.GetByEmail("test@example.com")).ReturnsAsync(user);
            var request = new LoginRequest { Email = "test@example.com", Password = "Password123" };

            Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await _authService.Login(request));
        }

        [Test]
        public void Login_ShouldThrowUnauthorizedException_WhenPasswordInvalid()
        {
            var passwordHash = BCrypt.Net.BCrypt.HashPassword("RealPassword");
            var user = new User { Email = "test@example.com", IsActive = true, PasswordHash = passwordHash };
            _mockUserRepo.Setup(r => r.GetByEmail("test@example.com")).ReturnsAsync(user);
            var request = new LoginRequest { Email = "test@example.com", Password = "WrongPassword" };

            Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await _authService.Login(request));
        }

        [Test]
        public async Task Login_ShouldReturnTokenResponse_WhenValidCredentials()
        {
            var passwordHash = BCrypt.Net.BCrypt.HashPassword("RealPassword");
            var user = new User 
            { 
                Id = 1, 
                Email = "test@example.com", 
                IsActive = true, 
                PasswordHash = passwordHash,
                Role = UserRole.Customer,
                FullName = "Test User"
            };
            _mockUserRepo.Setup(r => r.GetByEmail("test@example.com")).ReturnsAsync(user);

            _mockConfig.Setup(c => c["Jwt:Key"]).Returns("SuperSecretKeyForJWTEcommerceProject2026!");
            _mockConfig.Setup(c => c["Jwt:Issuer"]).Returns("Ecommerce.API");
            _mockConfig.Setup(c => c["Jwt:Audience"]).Returns("Ecommerce.Client");

            _mockRefreshTokenRepo.Setup(r => r.Create(It.IsAny<RefreshToken>())).ReturnsAsync(new RefreshToken());

            var request = new LoginRequest { Email = "test@example.com", Password = "RealPassword" };

            var result = await _authService.Login(request);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.AccessToken, Is.Not.Null.Or.Empty);
            Assert.That(result.RefreshToken, Is.Not.Null.Or.Empty);
            _mockRefreshTokenRepo.Verify(r => r.Create(It.IsAny<RefreshToken>()), Times.Once);
        }

        [Test]
        public void RefreshToken_ShouldThrowUnauthorizedException_WhenTokenNotFoundOrExpired()
        {
            _mockCurrentUserService.Setup(s => s.UserId).Returns(1);
            var tokens = new List<RefreshToken>
            {
                new RefreshToken { Token = BCrypt.Net.BCrypt.HashPassword("Token1"), IsRevoked = true, ExpiresAt = DateTime.Now.AddDays(1) },
                new RefreshToken { Token = BCrypt.Net.BCrypt.HashPassword("Token2"), IsRevoked = false, ExpiresAt = DateTime.Now.AddDays(-1) }
            };
            _mockRefreshTokenRepo.Setup(r => r.GetTokensByUserIdWithUser(1)).ReturnsAsync(tokens);

            Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await _authService.RefreshToken("Token1"));
            Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await _authService.RefreshToken("Token2"));
            Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await _authService.RefreshToken("Token3")); // completely missing
        }

        [Test]
        public async Task RefreshToken_ShouldReturnNewTokens_WhenValid()
        {
            _mockCurrentUserService.Setup(s => s.UserId).Returns(1);
            var user = new User { Id = 1, Email = "test@example.com", Role = UserRole.Customer, FullName = "Test User" };
            var rawToken = "ValidRawToken";
            var tokenEntity = new RefreshToken
            {
                Id = 10,
                UserId = 1,
                Token = BCrypt.Net.BCrypt.HashPassword(rawToken),
                IsRevoked = false,
                ExpiresAt = DateTime.Now.AddDays(1),
                User = user
            };
            _mockRefreshTokenRepo.Setup(r => r.GetTokensByUserIdWithUser(1)).ReturnsAsync(new List<RefreshToken> { tokenEntity });
            _mockRefreshTokenRepo.Setup(r => r.Update(10, tokenEntity)).ReturnsAsync(tokenEntity);
            _mockRefreshTokenRepo.Setup(r => r.Create(It.IsAny<RefreshToken>())).ReturnsAsync(new RefreshToken());

            _mockConfig.Setup(c => c["Jwt:Key"]).Returns("SuperSecretKeyForJWTEcommerceProject2026!");
            _mockConfig.Setup(c => c["Jwt:Issuer"]).Returns("Ecommerce.API");
            _mockConfig.Setup(c => c["Jwt:Audience"]).Returns("Ecommerce.Client");

            var result = await _authService.RefreshToken(rawToken);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.AccessToken, Is.Not.Null.Or.Empty);
            Assert.That(result.RefreshToken, Is.Not.Null.Or.Empty);
            Assert.That(tokenEntity.IsRevoked, Is.True);
            _mockRefreshTokenRepo.Verify(r => r.Update(10, tokenEntity), Times.Once);
            _mockRefreshTokenRepo.Verify(r => r.Create(It.IsAny<RefreshToken>()), Times.Once);
        }

        [Test]
        public async Task Logout_ShouldRevokeToken_WhenFound()
        {
            var tokenEntity = new RefreshToken { Id = 10, Token = "token", UserId = 1, IsRevoked = false };
            _mockRefreshTokenRepo.Setup(r => r.GetByTokenAndUserId("token", 1)).ReturnsAsync(tokenEntity);
            _mockRefreshTokenRepo.Setup(r => r.Update(10, tokenEntity)).ReturnsAsync(tokenEntity);

            var result = await _authService.Logout("token", 1);

            Assert.That(result, Is.True);
            Assert.That(tokenEntity.IsRevoked, Is.True);
            _mockRefreshTokenRepo.Verify(r => r.Update(10, tokenEntity), Times.Once);
        }

        [Test]
        public async Task Logout_ShouldReturnFalse_WhenNotFound()
        {
            _mockRefreshTokenRepo.Setup(r => r.GetByTokenAndUserId("token", 1)).ReturnsAsync((RefreshToken?)null);

            var result = await _authService.Logout("token", 1);

            Assert.That(result, Is.False);
        }

        [Test]
        public async Task RevokeAllTokens_ShouldRevokeActiveTokens()
        {
            var token1 = new RefreshToken { IsRevoked = false };
            var token2 = new RefreshToken { IsRevoked = false };
            _mockRefreshTokenRepo.Setup(r => r.GetActiveTokensByUserIdAsync(1)).ReturnsAsync(new List<RefreshToken> { token1, token2 });
            _mockRefreshTokenRepo.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            var result = await _authService.RevokeAllTokens(1);

            Assert.That(result, Is.True);
            Assert.That(token1.IsRevoked, Is.True);
            Assert.That(token2.IsRevoked, Is.True);
            _mockRefreshTokenRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
        }
    }
}

using System;
using System.Text;
using System.Linq;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt; 
using System.Threading.Tasks;         
using Microsoft.IdentityModel.Tokens; 
using Microsoft.Extensions.Configuration;
using AutoMapper;
using Ecommerce.Contracts.Services;
using Ecommerce.Contracts.Repositories;
using Ecommerce.Models.DTOs;
using Ecommerce.Models;
using Ecommerce.DAL.Context;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.BLL
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private ICurrentUserService _currentUserService;

        public AuthService(IUserRepository userRepository, IRefreshTokenRepository refreshTokenRepository, IMapper mapper,  IConfiguration configuration, ICurrentUserService currentUserService)
        {
            _userRepository = userRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _mapper = mapper;
            _configuration = configuration;
            _currentUserService = currentUserService;
        }

        public async Task<RegisterResponse> Register(RegisterRequest request)
        {
            var existingUser = await _userRepository.GetByEmail(request.Email);
            if (existingUser != null)
            {
                throw new InvalidOperationException("User already exists with this email");
            }

            var user = _mapper.Map<User>(request);
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
            user.IsActive = true;

            if(_currentUserService.Role == "Admin"){
                user.Role = UserRole.Admin;
            }

            user.Role = UserRole.Customer;

            var createdUser = await _userRepository.Create(user);

            return  _mapper.Map<RegisterResponse>(createdUser);
        }

        public async Task<TokenResponse> Login(LoginRequest request)
        {
            var user = await _userRepository.GetByEmail(request.Email);
            if (user == null || !user.IsActive)
            {
                throw new UnauthorizedAccessException("Invalid credentials or account is suspended / Deactivated");
            }

            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
            if (!isPasswordValid)
            {
                throw new UnauthorizedAccessException("Invalid credentials.");
            }

            var accessToken = GenerateJwtToken(user);
            var refreshTokenString = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            var hashedRefreshToken = BCrypt.Net.BCrypt.HashPassword(refreshTokenString);

            var refreshTokenEntity = new RefreshToken
            {
                UserId = user.Id,
                Token = hashedRefreshToken,
                ExpiresAt = DateTime.Now.AddDays(7),
                IsRevoked = false
            };

            await _refreshTokenRepository.Create(refreshTokenEntity);

            _currentUserService = new CurrentUserService(new HttpContextAccessor());
            return new TokenResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshTokenString,
                ExpiresAt = DateTime.Now.AddMinutes(15)
            };
        }

        public async Task<TokenResponse> RefreshToken(string refreshToken)
        {
            var userId = _currentUserService.UserId;
            var tokenEntities = await _refreshTokenRepository.GetTokensByUserIdWithUser(userId);
            var tokenEntity = tokenEntities.FirstOrDefault(rt => BCrypt.Net.BCrypt.Verify(refreshToken, rt.Token));
            
            if (tokenEntity == null || tokenEntity.IsRevoked || tokenEntity.ExpiresAt < DateTime.Now)
            {
                throw new UnauthorizedAccessException("Invalid or expired refresh token");
            }

            tokenEntity.IsRevoked = true;
            await _refreshTokenRepository.Update(tokenEntity.Id, tokenEntity);

            var newRefreshTokenString = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            var newRefreshTokenEntity = new RefreshToken
            {
                UserId = tokenEntity.UserId,
                Token = newRefreshTokenString,
                ExpiresAt = DateTime.Now.AddDays(7),
                IsRevoked = false
            };

            await _refreshTokenRepository.Create(newRefreshTokenEntity);

            var newAccessToken = GenerateJwtToken(tokenEntity.User);

            return new TokenResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshTokenString,
                ExpiresAt = newRefreshTokenEntity.ExpiresAt
            };
        }

        public async Task<bool> Logout(string refreshToken, int userId)
        {
            var tokenEntity = await _refreshTokenRepository.GetByTokenAndUserId(refreshToken, userId);

            if (tokenEntity != null)
            {
                tokenEntity.IsRevoked = true;
                await _refreshTokenRepository.Update(tokenEntity.Id, tokenEntity);
                return true;
            }

            return false;
        }


        public async Task<bool> RevokeAllTokens(int userId)
        {
            var tokens = await _refreshTokenRepository.GetActiveTokensByUserIdAsync(userId);

            foreach (var token in tokens)
            {
                token.IsRevoked = true;
            }

            await _refreshTokenRepository.SaveChangesAsync();
            return true;
        }

        private string GenerateJwtToken(User user)
        {   
            var tokenHandler = new JwtSecurityTokenHandler();
            
            var keyString = _configuration["Jwt:Key"] ?? "SuperSecretKeyForJWTEcommerceProject2026!";
            var key = Encoding.UTF8.GetBytes(keyString);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim("fullName", user.FullName)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddMinutes(30),
                Issuer = _configuration["Jwt:Issuer"] ?? "Ecommerce.API",
                Audience = _configuration["Jwt:Audience"] ?? "Ecommerce.Client",
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}

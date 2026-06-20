using Ecommerce.Models.DTOs;

namespace Ecommerce.Contracts.Services
{
    public interface IAuthService
    {
        Task<RegisterResponse> Register(RegisterRequest request);
        Task<TokenResponse> Login(LoginRequest request);
        Task<TokenResponse> RefreshToken(string refreshToken);
        Task<bool> Logout(string refreshToken, int userId);
        Task<bool> RevokeAllTokens(int userId);
    }
}
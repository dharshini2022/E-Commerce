using Ecommerce.Models;

namespace Ecommerce.Contracts.Repositories
{
    public interface IRefreshTokenRepository : IRepository<int, RefreshToken>
    {
        Task<RefreshToken?> GetByTokenAsync(string token);
        Task<ICollection<RefreshToken>> GetActiveTokensByUserIdAsync(int userId);
        Task<ICollection<RefreshToken>> GetTokensByUserIdWithUser(int userId);
        Task<RefreshToken?> GetByTokenAndUserId(string token, int userId);
    }
}

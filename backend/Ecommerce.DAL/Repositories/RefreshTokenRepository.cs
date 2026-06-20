using Ecommerce.Contracts.Repositories;
using Ecommerce.DAL.Context;
using Ecommerce.Models;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.DAL.Repositories
{
    public class RefreshTokenRepository : AbstractRepository<int, RefreshToken>, IRefreshTokenRepository
    {
        private readonly AppDbContext _dbContext;

        public RefreshTokenRepository(AppDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<RefreshToken?> GetByTokenAsync(string token)
        {
            return await _dbContext.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == token);
        }

        public async Task<ICollection<RefreshToken>> GetActiveTokensByUserIdAsync(int userId)
        {
            return await _dbContext.RefreshTokens
                .Where(rt => rt.UserId == userId && !rt.IsRevoked)
                .ToListAsync();
        }

        public async Task<ICollection<RefreshToken>> GetTokensByUserIdWithUser(int userId)
        {
            return await _dbContext.RefreshTokens
                .Include(rt => rt.User)
                .Where(rt => rt.UserId == userId)
                .ToListAsync();
        }

        public async Task<RefreshToken?> GetByTokenAndUserId(string token, int userId)
        {
            return await _dbContext.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == token && rt.UserId == userId);
        }
    }
}

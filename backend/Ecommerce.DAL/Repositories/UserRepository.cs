using Ecommerce.Contracts.Repositories;
using Ecommerce.DAL.Context;
using Ecommerce.Models;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.DAL.Repositories
{
    public class UserRepository : AbstractRepository<int, User>, IUserRepository
    {
        private readonly AppDbContext _dbContext;
        public UserRepository(AppDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<User?> GetByEmail(string email)
        {
            return await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<bool> VerifyEmailUnique(string email, int userId)
        {
            return !await _dbContext.Users.AnyAsync(u => u.Email == email && u.Id != userId);
        }

        public async Task<bool> ChangePassword(int userId, string newPassword)
        {
            var user = await GetById(userId);
            if (user == null) return false;
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            await Update(userId, user);
            return true;
        }


        public async Task<User?> CreateAdmin(User admin, int creatorId)
        {
            var creator = await GetById(creatorId);
            if (creator == null || creator.Role != UserRole.Admin) return null;
            admin.Role = UserRole.Admin;
            return await Create(admin);
        }

        public async Task<ICollection<UserAddress>> GetAddressByUserId(int userId)
        {
            return await _dbContext.UserAddresses.Where(x => x.UserId == userId).ToListAsync();
        }
        public async Task<ICollection<User>> GetAllByRole(UserRole role)
        {
            return await _dbContext.Users.Where(u => u.Role == role).ToListAsync();
        }

        public async Task<(ICollection<User>, int totalCount)> GetPagedUsers(string searchTerm, int pageNumber, int pageSize)
        {
            var query = _dbContext.Users.AsQueryable();
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                string lowerSearch = searchTerm.ToLower();
                query = query.Where(u => u.FullName.ToLower().Contains(lowerSearch)  || u.Email.ToLower().Contains(lowerSearch));
            }
            
            int totalCount = await query.CountAsync(); 

            var items = await query.OrderBy(u => u.FullName)
                .Skip((pageNumber - 1) * pageSize).Take(pageSize)
                .ToListAsync(); 

            return (items, totalCount);
        }

        public async Task<ICollection<UserAddress>> GetAllAddressByUserId(int userId)
        {
            return await _dbContext.UserAddresses.Where(u => u.UserId == userId).ToListAsync();
        }

        public async Task<UserAddress> AddUserAddress(UserAddress address)
        {
            await _dbContext.AddAsync(address);
            await _dbContext.SaveChangesAsync();
            return address;
        }
    }
}
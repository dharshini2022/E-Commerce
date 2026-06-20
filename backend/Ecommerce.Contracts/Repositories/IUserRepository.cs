using System.Net.Sockets;
using Ecommerce.Models;

namespace Ecommerce.Contracts.Repositories
{
    public interface IUserRepository : IRepository<int, User>
    {
        Task<User?> GetByEmail(string email);
        Task<bool> VerifyEmailUnique(string email, int userId);
        Task<ICollection<User>> GetAllByRole(UserRole role);
        Task<bool> ChangePassword(int userId, string newPassword);
        Task<User?> CreateAdmin(User admin, int creatorId);
        Task<(ICollection<User>, int totalCount)> GetPagedUsers(string searchTerm, int pageNumber, int pageSize);
        Task<ICollection<UserAddress>> GetAllAddressByUserId(int userId);
        Task<UserAddress> AddUserAddress(UserAddress address);
    }
}

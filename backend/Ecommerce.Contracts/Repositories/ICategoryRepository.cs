using Ecommerce.Models;

namespace Ecommerce.Contracts.Repositories
{
    public interface ICategoryRepository : IRepository<int, Category>
    {
        Task<ICollection<Category>> GetRootCategories();
        Task<ICollection<Category>> GetAllWithChildren();
        Task<Category?> GetBySlug(string slug);
        Task<bool> SlugExists(string slug);
        Task<bool> NameExists(string name);
        Task<ICollection<int>> GetAncestorIds(int id);
        Task<bool> HasProducts(int id);
        Task<ICollection<Category>> GetDescendants(int id);
        Task<int> GetProductCount(int id);
    }
}

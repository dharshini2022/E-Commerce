using Ecommerce.Contracts.Repositories;
using Ecommerce.DAL.Context;
using Ecommerce.Models;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.DAL.Repositories
{
    public class CategoryRepository : AbstractRepository<int, Category>, ICategoryRepository
    {
        private readonly AppDbContext _dbContext;

        public CategoryRepository(AppDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ICollection<Category>> GetAllWithChildren()
        {
            return await _dbContext.Categories.Include(c => c.Products).ToListAsync();
        }

        public override async Task<Category?> GetById(int id)
        {
            var category = await _dbContext.Categories.Include(c => c.Products).FirstOrDefaultAsync(c => c.Id == id);

            return category;
        }

        public async Task<Category?> GetBySlug(string slug)
        {
            return await _dbContext.Categories
                .Include(c => c.Children)
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.slug.Contains(slug));
        }

        public async Task<bool> SlugExists(string slug)
        {
            return await _dbContext.Categories.AnyAsync(c => c.slug == slug);
        }

        public async Task<bool> NameExists(string name)
        {
            return await _dbContext.Categories.AnyAsync(c => c.Name == name);
        }

        public async Task<ICollection<int>> GetAncestorIds(int id)
        {
            var ancestors = new List<int>();
            var current = await _dbContext.Categories.FindAsync(id);

            while (current?.ParentId != null)
            {
                ancestors.Add(current.ParentId.Value);
                current = await _dbContext.Categories.FindAsync(current.ParentId.Value);
            }

            return ancestors;
        }

        public async Task<bool> HasProducts(int id)
        {
            return await _dbContext.Products.AnyAsync(p => p.CategoryId == id);
        }

        public async Task<int> GetProductCount(int id)
        {
            return await _dbContext.Products.CountAsync(p => p.CategoryId == id);
        }

        public override async Task<Category?> Delete(int id)
        {
            var category = await GetById(id) ?? throw new KeyNotFoundException($"Category with ID {id} not found.");
            category.isActive = false;
            await _dbContext.SaveChangesAsync();
            return category;
        }

        public async Task<ICollection<Category>> GetDescendants(int id)
        {
            var root = await _dbContext.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (root == null) return new List<Category>();

            var result = new List<Category> { root };
            var queue = new Queue<int>();
            queue.Enqueue(id);

            while (queue.Count > 0)
            {
                var parentId = queue.Dequeue();
                var children = await _dbContext.Categories
                    .Include(c => c.Products)
                    .Where(c => c.ParentId == parentId)
                    .ToListAsync();

                foreach (var child in children)
                {
                    result.Add(child);
                    queue.Enqueue(child.Id);
                }
            }

            return result;
        }

        public async Task<ICollection<Category>> GetRootCategories()
        {
            return await _dbContext.Categories
                .Include(c => c.Products)
                .Where(c => c.ParentId == null && c.isActive)
                .ToListAsync();
        }
    }
}
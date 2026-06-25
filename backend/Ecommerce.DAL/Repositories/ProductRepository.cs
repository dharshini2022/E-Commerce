using Ecommerce.Contracts.Repositories;
using Ecommerce.DAL.Context;
using Ecommerce.Models;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.DAL.Repositories
{
    public class ProductRepository : AbstractRepository<int, Product>, IProductRepository
    {
        private readonly AppDbContext _dbContext;

        public ProductRepository(AppDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public override async Task<Product?> GetById(int id)
        {
            return await _dbContext.Products
                .Include(p => p.Vendor)
                .Include(p => p.Category)
                .Include(p => p.Variants.Where(v => v.IsActive))
                    .ThenInclude(v => v.VariantImages.OrderBy(img => img.ImageOrder))
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<int> GetProductsCount(int? categoryId)
        {
            var query = _dbContext.Products.Where(p => p.Status == ProductStatus.Active);

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }
            return await query.CountAsync();
        }

        public async Task<ICollection<Product>> GetProductsPaged(int pageNumber, int pageSize,string? sortBy, string? sortOrder,int? categoryId)
        {
            var query = _dbContext.Products
                .Include(p => p.Vendor)
                .Include(p => p.Category)
                .Include(p => p.Variants.Where(v => v.IsDefault))
                    .ThenInclude(v => v.VariantImages)
                .Where(p => p.Status == ProductStatus.Active && p.Variants.Any(v => v.IsDefault))
                .AsQueryable();

            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId.Value);

            bool isDesc = string.Equals(sortOrder?.Trim().ToLower(), "desc");

            query = sortBy?.Trim().ToLower() switch
            {
                "price" => isDesc
                    ? query.OrderByDescending(p => p.Variants.Where(v => v.IsActive).Min(v => v.Price))
                    : query.OrderBy(p => p.Variants.Where(v => v.IsActive).Min(v => v.Price)),

                "newest" => isDesc
                    ? query.OrderByDescending(p => p.CreatedAt)
                    : query.OrderBy(p => p.CreatedAt),

                "review" => isDesc
                    ? query.OrderByDescending(p => p.Reviews.Select(r => (double?)r.Rating).Average() ?? 0)
                    : query.OrderBy(p => p.Reviews.Select(r => (double?)r.Rating).Average() ?? 0),

                "rating" => isDesc
                    ? query.OrderByDescending(p => p.Reviews.Select(r => (double?)r.Rating).Average() ?? 0)
                    : query.OrderBy(p => p.Reviews.Select(r => (double?)r.Rating).Average() ?? 0),

                _ => query.OrderByDescending(p => p.CreatedAt)   
            };
            return await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

    
        public async Task<ICollection<Product>> SearchProductsByName(string query)
        {
            string lower = query.ToLower();
            return await _dbContext.Products
                .Include(p => p.Vendor)
                .Include(p => p.Category)
                .Include(p => p.Variants)
                    .ThenInclude(v => v.VariantImages)
                .Where(p => p.Status == ProductStatus.Active && p.Name.ToLower().Contains(lower))
                .ToListAsync();
        }

       
        public async Task<ICollection<Product>> GetProductsByVendorId(int vendorId)
        {
            return await _dbContext.Products
                .Include(p => p.Vendor)
                .Include(p => p.Category)
                .Include(p => p.Variants)
                    .ThenInclude(v => v.VariantImages)
                .Where(p => p.VendorId == vendorId)
                .ToListAsync();
        }


        public async Task<ICollection<Product>> GetActiveProducts()
        {
            return await _dbContext.Products
                .Include(p => p.Vendor)
                .Include(p => p.Category)
                .Include(p => p.Variants)
                    .ThenInclude(v => v.VariantImages)
                .Where(p => p.Status == ProductStatus.Active)
                .ToListAsync();
        }
    }
}

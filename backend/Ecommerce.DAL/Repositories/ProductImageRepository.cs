using Ecommerce.Contracts.Repositories;
using Ecommerce.DAL.Context;
using Ecommerce.Models;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.DAL.Repositories
{
    public class ProductImageRepository : AbstractRepository<int, ProductImage>, IProductImageRepository
    {
        private readonly AppDbContext _dbContext;

        public ProductImageRepository(AppDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ICollection<ProductImage>> GetImagesByVariantId(int variantId)
        {
            return await _dbContext.ProductImages
                .Where(img => img.VariantId == variantId)
                .OrderBy(img => img.ImageOrder)
                .ToListAsync();
        }

        public async Task<int?> GetVendorIdByImageId(int imageId)
        {
            return await _dbContext.ProductImages
                .Where(i => i.Id == imageId)
                .Select(i => i.ProductVariant.Product.VendorId)
                .FirstOrDefaultAsync();
        }
    }
}

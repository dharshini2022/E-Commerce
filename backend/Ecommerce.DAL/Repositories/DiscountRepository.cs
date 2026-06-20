using Ecommerce.Contracts.Repositories;
using Ecommerce.DAL.Context;
using Ecommerce.Models;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.DAL.Repositories
{
    public class DiscountRepository : AbstractRepository<int, Discount>, IDiscountRepository
    {
        private readonly AppDbContext _dbContext;

        public DiscountRepository(AppDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Discount?> GetByCode(string code)
        {
            return await _dbContext.Discounts.FirstOrDefaultAsync(d => d.Code == code);
        }

        public async Task<bool> ExistsBycode(string code)
        {
            return await _dbContext.Discounts.AnyAsync(d => d.Code == code);
        }

        public async Task<ICollection<Discount>> GetActiveDiscounts(int pageNumber, int pageSize,string searchTerm)
        {
            var query =  _dbContext.Discounts.Where(d => d.IsActive && d.ExpiresAt > DateTime.Now).AsQueryable();
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(d => d.Code.Contains(searchTerm));
            }
            return await query.Skip((pageNumber - 1) * pageSize).Take(pageSize) .ToListAsync();
        }

        public virtual async Task<ICollection<Discount>> GetDiscountHistory(int pageNumber, int pageSize,string searchTerm)
        {
            var query =  _dbContext.Discounts.AsQueryable();
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(d => d.Code.Contains(searchTerm));
            }
            return await query.Skip((pageNumber - 1) * pageSize).Take(pageSize) .ToListAsync();
        }

        public async Task<ICollection<Discount>> GetDiscountsByVendorId(int vendorId)
        {
            return await _dbContext.Discounts.Where(d => d.VendorId == vendorId).ToListAsync();
        }
        public async Task<ICollection<Discount>> GetDiscountsOfProduct(int productId, int vendorId, int categoryId)
        {
            return await _dbContext.Discounts.Where(d => 
                d.IsActive && 
                d.ExpiresAt > DateTime.Now && 
                d.UsedCount < d.UsageLimit &&
                ((d.Scope == DiscountScope.Product && d.ProductId == productId) ||
                (d.Scope == DiscountScope.Category && d.CategoryId == categoryId) ||
                (d.Scope == DiscountScope.Vendor && d.VendorId == vendorId))).ToListAsync();
        }

        public async Task<ICollection<Discount>> GetApplicableDiscountsAtCart(ICollection<int> ProductIDs, ICollection<int> CategoryIDs, ICollection<int> VendorIDs, decimal SubTotal)
        {
            return await _dbContext.Discounts.Where(d => d.IsActive && d.ExpiresAt > DateTime.Now 
                         && d.UsedCount < d.UsageLimit 
                         && d.MinOrderValue <= SubTotal
                         && (d.Scope == DiscountScope.Common 
                                || (d.Scope == DiscountScope.Product && d.ProductId.HasValue && ProductIDs.Contains(d.ProductId.Value))
                             
                                || (d.Scope == DiscountScope.Category && d.CategoryId.HasValue && CategoryIDs.Contains(d.CategoryId.Value))
                             
                                || (d.Scope == DiscountScope.Vendor && d.VendorId.HasValue && VendorIDs.Contains(d.VendorId.Value))
                            ))
                .ToListAsync();
        }
    }
}

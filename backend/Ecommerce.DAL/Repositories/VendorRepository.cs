using Ecommerce.Contracts.Repositories;
using Ecommerce.DAL.Context;
using Ecommerce.Models;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.DAL.Repositories
{
    public class VendorRepository : AbstractRepository<int, Vendor>, IVendorRepository
    {
        private readonly AppDbContext _dbContext;

        public VendorRepository(AppDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public override async Task<Vendor?> GetById(int key)
        {
            return await _dbContext.Vendors.Include(v => v.User).FirstOrDefaultAsync(v => v.Id == key);
        }

        public async Task<Vendor?> GetByUserId(int userId)
        {
            return await _dbContext.Vendors.Include(v => v.User).FirstOrDefaultAsync(v => v.UserId == userId);
        }

        public async Task<ICollection<Vendor>> GetByStoreName(string storeName)
        {
            return await _dbContext.Vendors.Where(v => v.StoreName.Contains(storeName)).ToListAsync();
        }

        public async Task<ICollection<Vendor>> GetVendorsByStatus(VendorStatus status)
        {
            return await _dbContext.Vendors.Include(v => v.User).Where(v => v.Status == status).ToListAsync();
        }

        public async Task<(ICollection<Vendor>, int totalCount)> GetPagedVendors(string searchTerm, int pageNumber, int pageSize)
        {
            var vendorQuery = _dbContext.Vendors.Include(v => v.User).AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                string lowerSearch = searchTerm.ToLower();
                vendorQuery = vendorQuery.Where(v => v.StoreName.ToLower().Contains(lowerSearch));
            }
            int totalCount = await vendorQuery.CountAsync();

            var items =  await vendorQuery.OrderBy(v => v.StoreName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return(items, totalCount);
        }

        public async Task<bool> VerifyGSTUnique(string gstNumber, int id = 0)
        {
            return !await _dbContext.Vendors.AnyAsync(v => v.GSTNumber == gstNumber && v.Id != id);
        }

        public async Task<bool> VerifyPANUnique(string panNumber, int id = 0)
        {
            return !await _dbContext.Vendors.AnyAsync(v => v.PANNumber == panNumber && v.Id != id);
        }

        public  async Task<Vendor?> ToggleVendorStatus(int id)
        {
            var existingVendor = await GetById(id);
            if (existingVendor == null) throw new KeyNotFoundException("Vendor not found.");
            existingVendor.IsActive = !existingVendor.IsActive;
            return await base.Update(id, existingVendor);
        }

        public async Task<bool> VerifyEmailUnique(string email, int id = 0)
        {
            return !await _dbContext.Vendors.AnyAsync(v => v.StoreEmail == email && v.Id != id);
        }
    }
}

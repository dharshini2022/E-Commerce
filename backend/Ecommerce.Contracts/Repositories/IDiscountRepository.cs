using Ecommerce.Models;

namespace Ecommerce.Contracts.Repositories
{
    public interface IDiscountRepository : IRepository<int, Discount>
    {
        Task<Discount?> GetByCode(string code);
        Task<bool> ExistsBycode(string code);
        Task<ICollection<Discount>> GetActiveDiscounts(int pageNumber, int pageSize,string searchTerm);
        Task<ICollection<Discount>> GetDiscountHistory(int pageNumber, int pageSize,string searchTerm);
        Task<ICollection<Discount>> GetDiscountsByVendorId(int vendorId);
        Task<ICollection<Discount>> GetDiscountsOfProduct(int porductId, int vendorId, int categoryId);
        Task<ICollection<Discount>> GetApplicableDiscountsAtCart(ICollection<int> ProductIDs, ICollection<int> CategoryIDs, ICollection<int> VendorIDs, decimal SubTotal);
    }
}

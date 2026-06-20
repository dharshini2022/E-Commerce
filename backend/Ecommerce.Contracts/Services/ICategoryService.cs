using Ecommerce.Models.DTOs;

namespace Ecommerce.Contracts.Services
{
    public interface ICategoryService
    {
        Task<CategoryResponse> Create(CreateCategoryRequest request);
        Task<CategoryResponse> Update(int id, UpdateCategoryRequest request);
        Task<CategoryStatusResponse> Delete(int id);
        Task<CategoryResponse?> GetById(int id);
        Task<ICollection<CategoryTreeResponse>> GetCategoryTree();
        Task<CategoryTreeResponse> GetBySlug(string slug);
        Task<ICollection<CategoryResponse>> GetFlatList();
    }
}

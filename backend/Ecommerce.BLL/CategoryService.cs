using AutoMapper;
using Ecommerce.Contracts.Repositories;
using Ecommerce.Contracts.Services;
using Ecommerce.Models;
using Ecommerce.Models.DTOs;
using Ecommerce.Shared.Exceptions;

namespace Ecommerce.BLL
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;

        public CategoryService(ICategoryRepository categoryRepository, IMapper mapper)
        {
            _mapper = mapper;
            _categoryRepository = categoryRepository;
        }

        public async Task<CategoryResponse> Create(CreateCategoryRequest request)
        {
            if (await SlugExists(request.Slug))    throw new UniquenessViolationException($"Slug '{request.Slug}' is already in use.");
            if(await _categoryRepository.NameExists(request.Name))     throw new UniquenessViolationException($"Category name '{request.Name}' is already in use.");

            var category = new Category
            {
                Name = request.Name.Trim().ToLower(),
                slug = request.Slug.Trim().ToLower(),
                ParentId = request.ParentId
            };

            var created = await _categoryRepository.Create(category);
            int productCount = await _categoryRepository.GetProductCount(created.Id);

            var result =  _mapper.Map<CategoryResponse>(created);
            result.ProductCount = productCount;
            return result;
        }

        public async Task<CategoryResponse> Update(int id, UpdateCategoryRequest request)
        {
            var existing = await _categoryRepository.GetById(id)  ?? throw new KeyNotFoundException($"Category with ID {id} not found.");

            if (request.ParentId.HasValue)
            {
                if (request.ParentId.Value == id)   throw new InvalidAncestorException("Cannot set a category as its own parent.");                    

                var ancestorIds = await _categoryRepository.GetAncestorIds(request.ParentId.Value);
                if (ancestorIds.Contains(id))   throw new InvalidAncestorException("Cannot set a descendant as the parent — this would create a cycle.");
                existing.ParentId = request.ParentId;
            }
            if (!string.IsNullOrWhiteSpace(request.Slug) && !string.Equals(existing.slug, request.Slug.Trim().ToLower()))
            {
                if (await SlugExists(request.Slug.Trim().ToLower()))   throw new UniquenessViolationException($"Slug '{request.Slug}' is already in use.");
                existing.slug = request.Slug.Trim().ToLower();
            }

            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                if (await _categoryRepository.NameExists(request.Name.Trim().ToLower()))   throw new UniquenessViolationException($"Name '{request.Name}' is already in use.");
                existing.Name = request.Name.Trim().ToLower();
            }

            var updated = await _categoryRepository.Update(id, existing);
            int productCount = await _categoryRepository.GetProductCount(id);

            var result =  _mapper.Map<CategoryResponse>(updated);
            result.ProductCount = productCount;
            return result;
        }
        public async Task<ICollection<CategoryTreeResponse>> GetCategoryTree()
        {
            var all = await _categoryRepository.GetAllWithChildren();

            var lookup = new Dictionary<int, CategoryTreeResponse>();

            foreach (var cat in all)
            {
                var value = _mapper.Map<CategoryTreeResponse>(cat);
                value.ProductCount = cat.Products.Count;
                lookup[cat.Id] = value;
            }

            var roots = new List<CategoryTreeResponse>();
            foreach (var cat in all)
            {
                if (cat.ParentId == null)
                {
                    roots.Add(lookup[cat.Id]);
                }
             }
            return roots;
        }
        public async Task<CategoryResponse?> GetById(int id)
        {
            var category = await _categoryRepository.GetById(id) ?? throw new KeyNotFoundException($"Category with ID {id} not found.");

            int productCount = await _categoryRepository.GetProductCount(id);
            var result = _mapper.Map<CategoryResponse>(category);
            result.ProductCount = productCount;
            return result;
        }

        public async Task<CategoryTreeResponse> GetBySlug(string slug)
        {
            var category = await _categoryRepository.GetBySlug(slug)  ?? throw new KeyNotFoundException($"Category with slug '{slug}' not found.");

            return _mapper.Map<CategoryTreeResponse>(category);
        }

        public async Task<CategoryStatusResponse> Delete(int id)
        {
            var existing = await _categoryRepository.GetById(id) ?? throw new KeyNotFoundException($"Category with ID {id} not found.");

            if (await _categoryRepository.HasProducts(id)) throw new InvalidOperationException("Cannot delete a category that has products assigned to it.");

            await _categoryRepository.Delete(id);
            return _mapper.Map<CategoryStatusResponse>(existing);
        }

        public async Task<ICollection<CategoryResponse>> GetFlatList()
        {
            var all = await _categoryRepository.GetAll();
            var result = new List<CategoryResponse>();
            foreach (var cat in all)
            {
                int productCount = await _categoryRepository.GetProductCount(cat.Id);
                var response = _mapper.Map<CategoryResponse>(cat);
                response.ProductCount = productCount;
                result.Add(response);
            }
            return result;
        }

        private async Task<bool> SlugExists(string slug)
        {
            return await _categoryRepository.SlugExists(slug.Trim().ToLower());
        }

    }
}

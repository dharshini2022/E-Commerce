using System.ComponentModel.DataAnnotations;
using AutoMapper;
using Ecommerce.Contracts.Repositories;
using Ecommerce.Contracts.Services;
using Ecommerce.Models;
using Ecommerce.Models.DTOs;
using NanoidDotNet;

namespace Ecommerce.BLL
{
    public class DiscountService : IDiscountService
    {
        private readonly IDiscountRepository _discountRepository;
        private readonly IVendorService _vendorService;
        private readonly ICategoryService _categoryService;
        private readonly IProductService _productService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMapper _mapper;
        public DiscountService(IDiscountRepository discountRepository, IVendorService vendorService,ICategoryService categoryService,IProductService productService, ICurrentUserService currentUserService, IMapper mapper)
        {
            _discountRepository = discountRepository;
            _vendorService = vendorService;
            _categoryService = categoryService;
            _productService = productService;
            _currentUserService = currentUserService;
            _mapper = mapper;
        }

        public async Task<DiscountResponse> CreateDiscount(CreateDiscountRequest request)
        {
            Console.WriteLine("Service start");
            await ValidateDiscountCreation(request);
            Console.WriteLine("Service Validation done");

            var discount = _mapper.Map<Discount>(request);
            discount.IsActive = true;
            discount.UsedCount = 0;
            Console.WriteLine("Service mapping done");
            
            await AuthorizeAndApplyScope(discount, request);
            Console.WriteLine("Discount Scope and Validations applied");
            
            discount.Code = await DiscountCodeGenerate();
            Console.WriteLine("Discount Code Generated");

            var result = await _discountRepository.Create(discount);
            Console.WriteLine("Discount Added to Database");
            return _mapper.Map<DiscountResponse>(result);
        }
        private async Task ValidateDiscountCreation(CreateDiscountRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Scope) || !Enum.TryParse<DiscountScope>(request.Scope, true, out _))
            {
                throw new ValidationException("Unsupported discount scope. Discount Scope can be common, vendor, product or category");
            }
            if (string.IsNullOrWhiteSpace(request.Type) || !Enum.TryParse<DiscountType>(request.Type, true, out _))
            {
                throw new ValidationException("Unsupported discount type. Discount Type can be percentage or flat");
            }
            if(request.ExpiresAt <= DateTime.Now)   throw new ValidationException("Expiry date must be in future");
            if(request.Type.Equals("percentage", StringComparison.OrdinalIgnoreCase) && request.Value > 100)  throw new ValidationException("Percentage discount can't exceed 100");
            if (request.Type.Equals("flat", StringComparison.OrdinalIgnoreCase) && request.Value > request.MinOrderValue)  throw new ValidationException("Flat discount value cannot exceed minimum order value");
        }
        private async Task AuthorizeAndApplyScope(Discount discount, CreateDiscountRequest request)
        {
            var role = _currentUserService.Role;

            discount.VendorId = null;
            discount.ProductId = null;
            discount.CategoryId = null;

            switch (request.Scope.ToLower())
            {
                case "common":
                    IsAdmin(role);
                    break;

                case "category":
                    IsAdmin(role);
                    if (!request.CategoryId.HasValue) throw new ValidationException("Category ID is required for Category discounts.");
                    var category = await _categoryService.GetById(request.CategoryId.Value) ?? throw new KeyNotFoundException("Category Not Found");
                    discount.CategoryId = request.CategoryId;
                    break;

                case "product":
                    IsAdmin(role);
                    if (!request.ProductId.HasValue) throw new ValidationException("Product ID is required for Product discounts.");
                    var product = await _productService.GetProductDetails(request.ProductId.Value) ?? throw new KeyNotFoundException("Product Not Found");
                    discount.ProductId = request.ProductId;
                    break;

                case "vendor":
                    if (role != "Vendor") throw new UnauthorizedAccessException("Only Vendors can create Vendor-specific discounts.");
                    
                    var vendor = await _vendorService.GetVendorByUserId(_currentUserService.UserId);
                    if (vendor == null) throw new ValidationException("Vendor profile not found for current user.");
                    
                    discount.VendorId = vendor.Id;
                    break;

                default:
                    throw new ValidationException("Unsupported discount scope. Discount Scope can be common, vendor, product or category");
            }
        }

        private void IsAdmin(string role)
        {
            if (role != "Admin") 
                throw new UnauthorizedAccessException("You don't have access to perform this operation.");
        }

        private async Task<string> DiscountCodeGenerate()
        {
            const string alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            string code = Nanoid.Generate(alphabet, 8);
            Console.WriteLine("Discount code generated: " + code);
            if(await _discountRepository.ExistsBycode(code))
            {
                return await DiscountCodeGenerate();
            }
            return code;
        }
        public async Task<ICollection<DiscountResponse>> GetActiveDiscounts(PageRequest request)
        {            
            var result =  await _discountRepository.GetActiveDiscounts(request.PageNumber, request.PageSize,request.SearchTerm ?? String.Empty);
            return _mapper.Map<ICollection<DiscountResponse>>(result);
        }
        public async Task<ICollection<DiscountResponse>> GetDiscountsOfProduct(int productId, int categoryId, int vendorId)
        {
            var discounts = await _discountRepository.GetDiscountsOfProduct(productId, categoryId, vendorId);

            return _mapper.Map<List<DiscountResponse>>(discounts);
        }
        public async Task<ICollection<DiscountResponse>> GetVendorDiscounts(int vendorId)
        {
            var result =  await _discountRepository.GetDiscountsByVendorId(vendorId);
            return _mapper.Map<ICollection<DiscountResponse>>(result);
        }    
        public async Task<ICollection<DiscountResponse>> GetAllDiscounts(PageRequest request)
        {
            var result = await _discountRepository.GetDiscountHistory(request.PageNumber, request.PageSize, request.SearchTerm?? String.Empty);
            return _mapper.Map<ICollection<DiscountResponse>>(result);
        }
        public async Task<ToggleDiscountStatusResponse> DeactivateDiscount(string Code)
        {
            var discount = await _discountRepository.GetByCode(Code);
            if (discount == null) throw new KeyNotFoundException("Discount Not Found");
            if(discount.Scope == DiscountScope.Vendor)
            {
                var vendor = await _vendorService.GetVendorByUserId(_currentUserService.UserId);
                if (vendor == null) throw new ValidationException("Vendor profile not found for current user.");
                if (discount.VendorId != vendor.Id) throw new UnauthorizedAccessException("You don't have access to perform this operation.");
            }
            else IsAdmin(_currentUserService.Role);
            
            discount.IsActive = false;
            discount.ExpiresAt = DateTime.Now;
            var result = await _discountRepository.Update(discount.Id, discount);
            return _mapper.Map<ToggleDiscountStatusResponse>(result);
        }
        public async Task<ICollection<DiscountCartResponse>> EvaluateCartDiscounts(CartEvaluationRequest request)
        {
            var productIds = request.Items.Select(i => i.ProductId).ToList();
            var categoryIds = request.Items.Select(i => i.CategoryId).ToList();
            var vendorIds = request.Items.Select(i => i.VendorId).ToList();

            var result = await _discountRepository.GetApplicableDiscountsAtCart(productIds, categoryIds, vendorIds, request.SubTotal);
            return _mapper.Map<ICollection<DiscountCartResponse>>(result);
            
        }

        public async Task<Discount> ValidateDiscount(string discountCode, ICollection<CartItem> eligibleItems, decimal subtotal){
            var discount = await _discountRepository.GetByCode(discountCode);
            if (discount == null || !discount.IsActive || discount.ExpiresAt < DateTime.Now)
                    throw new ValidationException( $"Discount code '{discountCode}' is invalid or expired.");

            if (discount.Scope == DiscountScope.Product && !eligibleItems.Any(e => e?.Variant?.Product?.Id == discount.ProductId)) {
                throw new ValidationException( $"Discount code '{discountCode}' is not applicable to the items in the cart.");
            } else if (discount.Scope == DiscountScope.Category && !eligibleItems.Any(e => discount.Category?.Id == e.Variant.Product.Category.Id)) {
                throw new ValidationException( $"Discount code '{discountCode}' is not applicable to the items in the cart.");
            } else if (discount.Scope == DiscountScope.Vendor && !eligibleItems.Any(e => discount.Vendor?.Id == e.Variant.Product.Vendor.Id)) {
                throw new ValidationException( $"Discount code '{discountCode}' is not applicable to the items in the cart.");
            }

            if(subtotal < discount.MinOrderValue){
                throw new ValidationException( $"Add Products worth Rs. {discount.MinOrderValue - subtotal} more to avail this Discount code '{discountCode}'!");
            }

            return discount;
        }

        public async Task<decimal> CalculateDiscountAmount(Discount discount, decimal subtotal){
            return discount.Type == DiscountType.Flat
                    ? discount.Value
                    : Math.Round(subtotal * discount.Value / 100, 2);
        }



    }
}

using System.Runtime.InteropServices;
using AutoMapper;
using Ecommerce.Models;
using Ecommerce.Models.DTOs;

namespace Ecommerce.BLL.Mapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            #region User
            CreateMap<User, UserProfileResponse>().ReverseMap();
            CreateMap<User, UserProfileRequest>().ReverseMap();
            CreateMap<User, RegisterRequest>().ReverseMap();
            CreateMap<User, RegisterResponse>().ReverseMap();
            #endregion

            #region UserAddress
            CreateMap<UserAddress,AddAddressRequest>().ReverseMap();
            #endregion

            #region vendor
            CreateMap<Vendor, CreateVendorRequest>().ReverseMap();
            CreateMap<Vendor, UpdateVendorRequest>().ReverseMap();
            CreateMap<Vendor, VendorProfileResponse>().ReverseMap();
            CreateMap<Vendor, VendorBasicResponse>().ReverseMap();
            CreateMap<Vendor, VendorStatusResponse>().ReverseMap();
            #endregion

            #region Category
            CreateMap<Category, CreateCategoryRequest>().ReverseMap();
            CreateMap<Category, UpdateCategoryRequest>().ReverseMap();
            CreateMap<Category, CategoryResponse>().ForMember(DTO => DTO.ProductCount, opt => opt.MapFrom(Model => Model.Products.Count)).ReverseMap();
            CreateMap<Category, CategoryTreeResponse>().ForMember(DTO => DTO.ProductCount, opt => opt.MapFrom(Model => Model.Products.Count)).ReverseMap();
            CreateMap<Category, CategoryStatusResponse>().ReverseMap();
            #endregion

            #region Products
            CreateMap<Product, CreateProductRequest>().ReverseMap();
            CreateMap<Product, ProductResponse>()
                .ForMember(DTO => DTO.StoreName,    opt => opt.MapFrom(Model => Model.Vendor.StoreName))
                .ForMember(DTO => DTO.CategoryName, opt => opt.MapFrom(Model => Model.Category.Name))
                .ForMember(DTO => DTO.Variants,     opt => opt.MapFrom(Model => Model.Variants))
                .ReverseMap();
            #endregion

            #region Product Variants
            CreateMap<ProductVariant, AddProductVariantRequest>().ReverseMap();
            CreateMap<ProductVariant, UpdateProductVariantRequest>().ReverseMap();
            CreateMap<ProductVariant, ProductVariantResponse>()
                .ForMember(DTO => DTO.VariantImages, opt => opt.MapFrom(Model => Model.VariantImages))
                .ReverseMap();
            #endregion
            
            #region Product Images
            CreateMap<ProductImage, CreateProductImageRequest>().ReverseMap();
            CreateMap<ProductImage, ProductImageResponse>().ReverseMap();
            #endregion

            #region Cart
            CreateMap<Cart, CartResponse>()
            .ForMember(dest => dest.TotalItems,opt => opt.MapFrom(src =>src.Items.Sum(i => i.Quantity)))
            .ForMember(dest => dest.TotalAmount,opt => opt.MapFrom(src =>src.Items.Sum(i =>i.Quantity * i.Variant.Price)))
            .ReverseMap();
            
            CreateMap<CartItem, CartItemResponse>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Variant.Product.Name))
            .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.Variant.ProductId))
            .ForMember(dest => dest.UnitPrice, opt => opt.MapFrom(src => src.Variant.Price))
            .ForMember(dest => dest.SubTotal, opt => opt.MapFrom(src => src.Quantity * src.Variant.Price))
            .ForMember(dest => dest.IsInStock, opt => opt.MapFrom(src => src.Variant.IsActive && src.Variant.StockQty > 0))
            .ReverseMap();

            CreateMap<CartItem, CartItemEvaluationResponse>()
            .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.Variant.Product.CategoryId))
            .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.Variant.ProductId))
            .ForMember(dest => dest.VendorId, opt => opt.MapFrom(src => src.Variant.Product.VendorId))
            .ForMember(dest => dest.SubTotal, opt => opt.MapFrom(src => src.Quantity * src.Variant.Price))
            .ReverseMap();

            CreateMap<CartItem,CartItemDeletionResponse>().ReverseMap();
            #endregion

            #region Wishlist
            CreateMap<WishlistItem, WishListItemResponse>()
            .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.Variant.ProductId))
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Variant.Product.Name))
            .ForMember(dest => dest.UnitPrice, opt => opt.MapFrom(src => src.Variant.Price))
            .ForMember(dest => dest.IsInStock,opt => opt.MapFrom(src => src.Variant.IsActive && src.Variant.StockQty > 0))
            .ReverseMap();
            #endregion

            #region Discount
            CreateMap<Discount, CreateDiscountRequest>().ReverseMap();
            CreateMap<Discount, DiscountResponse>().ReverseMap();
            CreateMap<Discount, DiscountCartResponse>().ReverseMap();
            CreateMap<Discount,ToggleDiscountStatusResponse>().ReverseMap();
            #endregion

            #region Order
            CreateMap<Order, OrderSummaryResponse>()
                .ForMember(dest => dest.OrderId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.User != null ? src.User.FullName : string.Empty))
                .ForMember(dest => dest.PaymentStatus, opt => opt.MapFrom(src => src.OrderPaymentStatus))
                .ReverseMap();
            CreateMap<OrderItem, OrderItemDTO>()
                .ForMember(DTO => DTO.ProductName, opt => opt.MapFrom(Model => Model.Variant.Product.Name))
                .ForMember(DTO => DTO.VendorStoreName, opt => opt.MapFrom(Model => Model.Vendor.StoreName))
                .ForMember(DTO => DTO.ProductId, opt => opt.MapFrom(Model => Model.Variant.ProductId))
                .ReverseMap();

            CreateMap<Shipment, ShipmentResponseDTO>().ReverseMap();
            #endregion

            #region Payment & Settlements
            CreateMap<Payment, PaymentResponseDTO>()
                .ForMember(dest => dest.OrderId, opt => opt.MapFrom(src => src.Order != null ? src.Order.Id : (int?)null))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ReverseMap();

            CreateMap<VendorSettlement, VendorSettlementDTO>()
                .ForMember(dest => dest.VendorStoreName, opt => opt.MapFrom(src => src.Vendor != null ? src.Vendor.StoreName : string.Empty))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ReverseMap();
            #endregion

            #region return
            CreateMap<Return, ReturnRequest>().ReverseMap();
            CreateMap<Return, ReturnSummaryDTO>().ReverseMap();
            CreateMap<ReturnItem, ReturnItemRequest>().ReverseMap();
            CreateMap<ReturnItem, ReturnItemDTO>()
                .ForMember(DTO => DTO.ProductName, opt => opt.MapFrom(Model => Model.OrderItem.Variant.Product.Name))
                .ReverseMap();
            #endregion

            #region Reviews
            CreateMap<Review, CreateReviewRequest>().ReverseMap();
            CreateMap<Review, UpdateReviewRequest>().ReverseMap();
            CreateMap<Review, ReviewDTO>()
                .ForMember(DTO => DTO.ProductName, opt => opt.MapFrom(Model => Model.Product.Name))
                .ForMember(DTO => DTO.UserFullName, opt => opt.MapFrom(Model => Model.User.FullName))
                .ForMember(DTO => DTO.ReviewImages, opt => opt.MapFrom(Model => Model.ReviewImages.Select(ri => ri.ImageUrl).ToList()))
                .ReverseMap();
            #endregion
        }

        private static string? MaskEmail(string? email)
        {
            if (string.IsNullOrWhiteSpace(email)) return email;
            var parts = email.Split('@');
            if (parts.Length != 2) return email;
            var name = parts[0];
            var domain = parts[1];
            if (name.Length <= 2)
            {
                return new string('*', name.Length) + "@" + domain;
            }
            return name[0] + new string('*', name.Length - 2) + name[^1] + "@" + domain;
        }

        private static string? MaskPhone(string? phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return phone;
            if (phone.Length <= 4)
            {
                return new string('*', phone.Length);
            }
            return new string('*', phone.Length - 4) + phone[^4..];
        }
    }
}
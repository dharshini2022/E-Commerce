# Multi-Vendor E-Commerce Platform — Architecture Document

---

## 1. Project Overview

A **Multi-Vendor E-Commerce Platform** built on **.NET 10 Web API** with PostgreSQL. The system supports three roles — **Customer**, **Vendor**, and **Admin** — with strict Role-Based Access Control (RBAC). It is designed using **Clean / Onion Architecture** principles, ensuring each layer communicates through abstractions and never leaks implementation details across boundaries.

---

## 2. Solution Structure

```
Ecommerce/
├── Ecommerce.slnx                  # Solution definition
├── Ecommerce.API/                  # Presentation Layer (HTTP endpoints)
├── Ecommerce.BLL/                  # Business Logic Layer (Service implementations)
├── Ecommerce.Contracts/            # Interface Contracts + DTOs (Pure Abstractions)
├── Ecommerce.DAL/                  # Data Access Layer (EF Core + Repository implementations)
├── Ecommerce.Models/               # Domain models + Enums (Plain C# classes)
├── Ecommerce.Shared/               # Cross-cutting utilities (Future use)
└── Requirements/
    ├── Functions.md
    ├── Technical_Design_Document.md
    └── Database_Models_and_Schema_Specification.md
```

---

## 3. Architectural Pattern

The application implements **Clean Architecture (Onion Architecture)** layered as follows:

```
┌─────────────────────────────────────────────┐
│              Ecommerce.API                  │  ← Presentation Layer
│     (Controllers, Program.cs, Swagger)      │
└────────────────────┬────────────────────────┘
                     │ depends on (via DI)
┌────────────────────▼────────────────────────┐
│              Ecommerce.BLL                  │  ← Business Logic Layer
│  (Service Implementations, AutoMapper)      │
└────────┬──────────────────────┬─────────────┘
         │ depends on           │ depends on
┌────────▼────────┐   ┌────────▼────────────────┐
│  Ecommerce.DAL  │   │  Ecommerce.Contracts     │  ← Interface Contracts + DTOs
│  (Repositories  │   │  (IRepository, IService  │
│   EF Core       │   │   Repository Interfaces  │
│   AppDbContext) │   │   Service Interfaces     │
└────────┬────────┘   │   DTOs)                  │
         │            └─────────────────────────┘
┌────────▼────────┐
│ Ecommerce.Models│  ← Domain Models (deepest / no dependencies)
│ (Pure C# POCO   │
│  models + Enums)│
└─────────────────┘
```

### Dependency Flow Rule
> `API → BLL → Contracts ← DAL`  
> Every layer depends **inward** on abstractions, never outward on concrete implementations.

---

## 4. Layer Responsibilities

### 4.1 `Ecommerce.Models` — Domain Models
Contains all database-mapped **Plain Old C# Objects (POCOs)** and their enumerations. No logic, no dependencies.

| Model | Enums |
|:---|:---|
| `User` | `UserRole` (Customer, Vendor, Admin) |
| `RefreshToken` | — |
| `UserAddress` | — |
| `Vendor` | `VendorStatus` (Pending, Approved, Suspended) |
| `Category` | — |
| `Product` | `ProductStatus` (Draft, Active, Archived) |
| `ProductVariant` | — |
| `ProductImage` | — |
| `Cart`, `CartItem` | — |
| `Discount` | `DiscountType` (Percentage, Flat) |
| `Order`, `OrderItem` | `OrderStatus`, `OrderPaymentStatus` |
| `Shipment` | `ShipmentStatus` (Pending, Initiated, Delivered, Picked, Cancelled) |
| `Payment` | `PaymentStatus` (Pending, Paid, Failed, Refunded) |
| `Return`, `ReturnItem` | `ReturnStatus`, `ReturnItemStatus`, `ReturnItemRefundStatus` |
| `Review`, `ReviewImage` | — |
| `Wishlist`, `WishlistItem` | — |
| `Notification` | `NotificationType`, `NotificationLevel` |
| `VendorSettlement` | `SettlementStatus` (Pending, Paid, Failed) |
| `DuplicateProductVariant` | — |

---

### 4.2 `Ecommerce.Contracts` — Interfaces + DTOs
The **central abstraction project**. All other layers depend on this. Defines:

#### Generic Repository Base
```
IRepository<K, T>
  ├── Create(T item) → Task<T>
  ├── GetById(K key) → Task<T?>
  ├── GetAll()       → Task<ICollection<T>>
  ├── Update(K, T)   → Task<T?>
  └── Delete(K)      → Task<T?>
```

#### Repository Interfaces (Ecommerce.Contracts/Repositories/)
Each extends `IRepository<int, T>` with domain-specific query methods:

| Interface | Key Extra Methods |
|:---|:---|
| `IUserRepository` | `GetByEmailAsync`, `GetByEmail`, `GetAddressByUserId` |
| `IRefreshTokenRepository` | `GetByTokenAsync`, `GetActiveTokensByUserIdAsync` |
| `IUserAddressRepository` | `GetAddressesByUserIdAsync` |
| `IVendorRepository` | `GetByUserIdAsync`, `GetByStoreNameAsync`, `GetVendorsByStatusAsync`, `VerifyGSTUniqueAsync` |
| `ICategoryRepository` | `GetRootCategoriesAsync`, `GetSubCategoriesAsync` |
| `IProductRepository` | `GetProductsPagedAsync`, `SearchProductsByNameAsync`, `GetProductsByVendorIdAsync` |
| `IProductVariantRepository` | `GetVariantsByProductIdAsync`, `GetDefaultVariantAsync`, `UpdateStockAsync` |
| `IDiscountRepository` | `GetByCodeAsync`, `GetActiveDiscountsAsync`, `GetDiscountsByVendorIdAsync` |
| `IOrderRepository` | `GetOrdersByUserId`, `GetOrdersByVendorId`, `GetOrdersByProductIdAsync`, `GetActiveVendorOrdersAsync` |
| `IShipmentRepository` | `GetShipmentsByVendorIdAsync`, `GetShipmentByTrackingNumberAsync` |
| `IPaymentRepository` | `GetByTransactionIdAsync`, `GetPaymentHistoryByUserIdAsync` |
| `IReturnRepository` | `GetReturnsByUserIdAsync`, `GetReturnsByVendorIdAsync`, `GetUnapprovedReturnsAsync` |
| `IReviewRepository` | `GetReviewsByProductIdAsync`, `GetReviewsByUserIdAsync`, `GetReviewsByVendorIdAsync` |
| `INotificationRepository` | `GetNotificationsByUserIdAsync`, `MarkAllAsReadAsync` |
| `IVendorSettlementRepository` | `GetSettlementsByVendorIdAsync`, `GetSettlementsByStatusAsync` |

#### Service Interfaces (Ecommerce.Contracts/Services/)

| Interface | Responsibility |
|:---|:---|
| `IAuthService` | Register, Login, RefreshToken, Logout, VerifyEmail, ForgotPassword, ResetPassword |
| `IUserService` | Profile management, Admin user controls (Suspend, Reactivate, ChangeRole) |
| `IProductService` | Catalog browsing (paged, filtered, sorted, searched), Vendor product/variant/image CRUD |
| `ICartService` | Cart view, AddToCart, UpdateQuantity, RemoveFromCart |
| `IOrderService` | Checkout transaction (payment, commission splits, stock reduction), Order history |
| `IReturnService` | Return requests, Admin approval/rejection, Pickup scheduling |
| `IReviewService` | Review CRUD, Product/Vendor reviews, Admin review overview |
| `IShipmentService` | Shipment schedule, Status transitions, Return pickup |
| `IDiscountService` | Active discounts, Vendor/Admin discount creation |
| `INotificationService` | Email OTP, Email confirmation, In-app notification creation |
| `IWishlistService` | Add/Remove/View wishlist items |

#### DTO Namespace (Ecommerce.Contracts/DTOs/)
Shared Data Transfer Objects used across all layers:

| DTO File | Contents |
|:---|:---|
| `AuthDTOs.cs` | `RegisterRequest`, `RegisterResponse`, `LoginRequest`, `TokenResponse`, `UserProfileDto`, `UpdateProfileRequest`, `ChangePasswordRequest`, `ResetPasswordRequest`, `UserListQuery`, `UserListItemDto`, `Page<T>` |
| `VendorDTOs.cs` | `VendorRegisterDTO`, `UpdateStoreDetailsDTO`, `VendorProfileDTO` |
| `ProductDTOs.cs` | `ProductFilterRequest`, `CreateProductRequest`, `ProductResponse`, `ProductVariantResponse`, `ProductImageResponse`, `CategoryCreateRequest` |
| `OrderDTOs.cs` | `PlaceOrderRequest`, `OrderSummaryResponse`, `OrderItemDTO`, `ReturnRequest`, `ReturnSummaryDTO`, `ReturnItemDTO`, `AdminRevenueDTO` |
| `ReviewDTOs.cs` | `CreateReviewRequest`, `UpdateReviewRequest`, `ReviewDTO` |

---

### 4.3 `Ecommerce.DAL` — Data Access Layer

#### `AppDbContext` (`Context/AppDbContext.cs`)
Entity Framework Core `DbContext` with:
- All 20+ `DbSet<T>` properties mapped to PostgreSQL tables
- **Fluent API** configurations (cascade/restrict delete behaviors)
- **JSONB** column mapping for `ProductVariant.AvailableValues`
- **Enum-as-string** conversions for status columns
- **snake_case** naming conventions via `EFCore.NamingConventions`

#### Repository Implementations
```
AbstractRepository<K, T> : IRepository<K, T>
  └── Generic CRUD using EF Core DbContext.Set<T>()
```
| Concrete Repository | Status |
|:---|:---|
| `AbstractRepository<K,T>` | ✅ Implemented |
| `UserRepository` | ✅ Implemented (extends `AbstractRepository` + `IUserRepository`) |
| All other repositories | ⬜ Pending implementation |

---

### 4.4 `Ecommerce.BLL` — Business Logic Layer

The BLL contains **service implementations** that use repositories (via injected interfaces) and AutoMapper to transform data.

#### AutoMapper Configuration (`Mapper/MappingProfile.cs`)
Full mapping definitions using `CreateMap<Source, Destination>().ReverseMap()`:

| Group | Mappings |
|:---|:---|
| User | `User ↔ UserProfileDto`, `User ↔ UserListItemDto`, `User ↔ RegisterRequest`, `User ↔ UpdateProfileRequest` |
| Vendor | `Vendor ↔ VendorRegisterDTO`, `Vendor ↔ UpdateStoreDetailsDTO`, `Vendor ↔ VendorProfileDTO` |
| Product | `Product ↔ CreateProductRequest`, `Product → ProductResponse` (with `ForMember` for `VendorStoreName`, `CategoryName`) |
| ProductVariant | `ProductVariant ↔ AddProductVariantRequest`, `ProductVariant ↔ UpdateProductVariantRequest`, `ProductVariant ↔ ProductVariantResponse` |
| ProductImage | `ProductImage ↔ CreateProductImageRequest`, `ProductImage ↔ ProductImageResponse` |
| Order | `Order ↔ OrderSummaryResponse`, `OrderItem → OrderItemDTO` (with `ForMember` for `ProductName`, `VendorStoreName`) |
| Return | `Return ↔ ReturnRequest`, `Return ↔ ReturnSummaryDTO`, `ReturnItem ↔ ReturnItemRequest`, `ReturnItem → ReturnItemDTO` (with deep join for `ProductName`) |
| Review | `Review ↔ CreateReviewRequest`, `Review ↔ UpdateReviewRequest`, `Review → ReviewDTO` (with `ForMember` for `ProductName`, `UserFullName`, `ReviewImages`) |

#### Service Implementations
| Service | Status |
|:---|:---|
| `UserService` | 🔄 Partial (`GetUserDetails`, `UpdateProfile` done; other methods stubs) |
| All other services | ⬜ Pending implementation |

---

### 4.5 `Ecommerce.API` — Presentation Layer

#### `Program.cs` (Service Registrations)
| Region | What is Registered |
|:---|:---|
| `#region Contexts` | `AppDbContext` with PostgreSQL connection string (`"Default"`) |
| `#region Automapper` | `MappingProfile` scanned from `Ecommerce.BLL.Mapper` |
| Built-in | `AddOpenApi()`, `Swashbuckle`, JWT Bearer Auth |

**To be added** (pending implementation):
- `AddScoped<IUserRepository, UserRepository>()`
- `AddScoped<IAuthService, AuthService>()`
- etc. for all services and repositories

---

## 5. Technology Stack

| Area | Technology |
|:---|:---|
| **Runtime** | .NET 10 |
| **Web Framework** | ASP.NET Core Web API |
| **Database** | PostgreSQL |
| **ORM** | Entity Framework Core 10 (Code-First, Fluent API) |
| **Authentication** | JWT Tokens (7-day expiry) + BCrypt password hashing |
| **Object Mapping** | AutoMapper 16 |
| **API Documentation** | Swagger (Swashbuckle + OpenAPI) |
| **Email** | SMTP |
| **Real-time Push** | SignalR (planned) |
| **Logging** | SeriLog - File-based (planned) |
| **Testing** | NUnit (planned) |
| **PDF Export** | ClosedXML (planned) |
| **Frontend** | Angular (separate project) |

---

## 6. Design Patterns Applied

| Pattern | Where Used |
|:---|:---|
| **Repository Pattern** | `IRepository<K,T>` → `AbstractRepository` → Specific Repositories |
| **Service Layer Pattern** | `IXxxService` → `XxxService` in BLL |
| **Dependency Inversion** | All layers depend on `Ecommerce.Contracts` interfaces, not concrete classes |
| **Dependency Injection** | Builder pattern via `Program.cs` (`builder.Services.AddScoped<I, C>()`) |
| **Generic Repository** | `AbstractRepository<K,T>` covers all CRUD generically |
| **AutoMapper (DTO Mapping)** | Centralised in `MappingProfile.cs`, handles model↔DTO transformations |
| **Builder Pattern** | `WebApplication.CreateBuilder(args)` in `Program.cs` |
| **Scoped Lifetime** | All services and repositories registered as `Scoped` |

---

## 7. Database Schema Summary

| Table | Primary Entity |
|:---|:---|
| `users` | `User` |
| `refresh_tokens` | `RefreshToken` |
| `user_addresses` | `UserAddress` |
| `vendors` | `Vendor` |
| `categories` | `Category` (self-referencing tree) |
| `products` | `Product` |
| `product_variants` | `ProductVariant` (JSONB `AvailableValues`) |
| `product_images` | `ProductImage` |
| `carts` | `Cart` |
| `cart_items` | `CartItem` |
| `discounts` | `Discount` |
| `orders` | `Order` |
| `order_items` | `OrderItem` |
| `shipments` | `Shipment` |
| `payments` | `Payment` |
| `returns` | `Return` |
| `return_items` | `ReturnItem` |
| `reviews` | `Review` |
| `review_images` | `ReviewImage` |
| `wishlists` | `Wishlist` |
| `wishlist_items` | `WishlistItem` |
| `notifications` | `Notification` |
| `vendor_settlements` | `VendorSettlement` |

---

## 8. Functional Coverage Traceability

| Functions.md Feature | Interface | Status |
|:---|:---|:---|
| Register / Email Verification | `IAuthService.Register` | ⬜ Pending |
| Login / JWT Token | `IAuthService.Login` | ⬜ Pending |
| Update Profile / Change Password | `IUserService.UpdateProfile`, `ChangePassword` | 🔄 Partial |
| Add/View User Addresses | `IUserAddressRepository` | ⬜ Pending |
| View Discounts (active) | `IDiscountService.GetActiveDiscounts` | ⬜ Pending |
| View Cart | `ICartService.GetCartByUserId` | ⬜ Pending |
| Add to Cart / Remove / Update Qty | `ICartService` | ⬜ Pending |
| Place Order (full workflow) | `IOrderService.PlaceOrder` | ⬜ Pending |
| View Orders | `IOrderService.GetUserOrderHistory` | ⬜ Pending |
| Cancel Order | `IOrderService.CancelOrder` | ⬜ Pending |
| Return Order | `IReturnService.RequestReturn` | ⬜ Pending |
| Review Product | `IReviewService.AddReview` | ⬜ Pending |
| View Wishlist / Add / Remove | `IWishlistService` | ⬜ Pending |
| Vendor Register (GST/PAN) | `IAuthService.Register` (VendorDTO) | ⬜ Pending |
| Vendor: Manage Products/Variants | `IProductService` | ⬜ Pending |
| Admin: Approve/Suspend Vendor | `IUserService` | ⬜ Pending |
| Admin: Create Admin User | `UserRepository.CreateAdmin` | ✅ DAL done |
| Admin: Approve/Reject Returns | `IReturnService.ApproveReturn` | ⬜ Pending |
| Shipment Scheduling (2-hr delay) | `IShipmentService.ScheduleShipment` | ⬜ Pending |
| Push Notification (SignalR) | `INotificationService.CreateNotification` | ⬜ Pending |
| Email OTP Verification | `INotificationService.SendEmailVerificationOtp` | ⬜ Pending |
| Vendor Settlements | `IOrderService.GetVendorSettlements` | ⬜ Pending |
| Admin Revenue Dashboard | `IOrderService.GetAdminRevenue` | ⬜ Pending |
| Product Catalog (Paged/Sorted/Filtered) | `IProductService.GetProductsCatalog` | ⬜ Pending |
| Product Search (by name) | `IProductService.SearchProducts` | ⬜ Pending |

---

## 9. What Remains To Build

| Area | Action Required |
|:---|:---|
| **Repository Implementations** | Create concrete repo classes in `Ecommerce.DAL/Repositories/` for all 22 remaining entities |
| **Service Implementations** | Implement all service interfaces in `Ecommerce.BLL/Services/` |
| **API Controllers** | Create controllers in `Ecommerce.API/Controllers/` for all endpoints |
| **DI Registration** | Register all repos and services in `Program.cs` |
| **JWT Middleware** | Configure `UseAuthentication` + `UseAuthorization` in `Program.cs` |
| **Background Jobs** | Shipment status transitions (2-hr → Initiated, 2-day → Delivered/Picked) |
| **SignalR Hub** | Real-time push notification hub |
| **SeriLog** | File-based structured logging middleware |
| **Exception Filters** | Global exception middleware |
| **Rate Limiting** | API request rate limiting |
| **NUnit Tests** | Unit tests for BLL services with mocked repositories |

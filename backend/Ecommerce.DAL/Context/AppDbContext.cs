using Ecommerce.Models;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.Reflection.Metadata;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;

namespace Ecommerce.DAL.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<UserAddress> UserAddresses => Set<UserAddress>();
        public DbSet<Vendor> Vendors => Set<Vendor>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<ProductImage> ProductImages => Set<ProductImage>();
        public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
        public DbSet<Cart> Carts => Set<Cart>();
        public DbSet<CartItem> CartItems => Set<CartItem>();
        public DbSet<Discount> Discounts => Set<Discount>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();
        public DbSet<Return> Returns => Set<Return>();
        public DbSet<ReturnItem> ReturnItems => Set<ReturnItem>();
        public DbSet<Review> Reviews => Set<Review>();
        public DbSet<Wishlist> Wishlists => Set<Wishlist>();
        public DbSet<WishlistItem> WishlistItems => Set<WishlistItem>();
        public DbSet<Notification> Notifications => Set<Notification>();
        public DbSet<StockReservation> StockReservations => Set<StockReservation>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Email).IsRequired();
                entity.HasIndex(c => c.Email).IsUnique();
                entity.Property(c => c.PasswordHash);
                entity.Property(c => c.FullName).IsRequired().HasMaxLength(120);
                entity.Property(x => x.Role).HasConversion<string>().IsRequired();
                entity.Property(c => c.CreatedAt).HasColumnType("timestamp without time zone");
            });

            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.ToTable("refresh_tokens");
                entity.HasKey(c => c.Id);
                entity.Property(x => x.Token).IsRequired().HasMaxLength(512);
                entity.HasIndex(x => x.Token).IsUnique(); 
                entity.Property(x => x.ExpiresAt).HasColumnType("timestamp without time zone");
                
                //Relation (User to Refresh Token => One to Many)
                entity.HasOne(x => x.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict); 
            });

            modelBuilder.Entity<UserAddress>(entity =>
            {
                entity.ToTable("user_addresses");
                entity.HasKey(x => x.Id);           
                entity.Property(x => x.RecipientName).IsRequired().HasMaxLength(120);
                entity.Property(x => x.Phone).IsRequired().HasMaxLength(10);
                entity.Property(x => x.Line1).IsRequired().HasMaxLength(150);
                entity.Property(x => x.Line2).HasMaxLength(150);
                entity.Property(x => x.Landmark).HasMaxLength(120);
                entity.Property(x => x.City).IsRequired().HasMaxLength(80);
                entity.Property(x => x.State).IsRequired().HasMaxLength(80);
                entity.Property(x => x.PostalCode).IsRequired().HasMaxLength(6);   
                entity.Property(x => x.Label).HasMaxLength(40);

                //Relation(User to UserAddress => One to Many)
                entity.HasOne(x => x.User)
                .WithMany(u => u.Addresses)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => x.UserId);  
            });

            modelBuilder.Entity<Vendor>(entity =>
            {
                entity.ToTable("vendors");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.StoreName).IsRequired().HasMaxLength(120);
                entity.HasIndex(x => x.StoreName);
                entity.Property(x => x.StoreEmail).HasMaxLength(100);
                entity.Property(x => x.GSTNumber).IsRequired().HasMaxLength(15);
                entity.Property(x => x.PANNumber).IsRequired().HasMaxLength(10);
                entity.Property(x => x.Description).HasMaxLength(2000);
                entity.Property(x => x.LogoUrl).HasMaxLength(500);
                entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
                entity.Property(x => x.ApprovedAt).HasColumnType("timestamp without time zone");

                // Relation (Vendor to User => one to one)
                entity.HasOne(x => x.User)
                .WithOne(u => u.Vendor)
                .HasForeignKey<Vendor>(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);  
                entity.HasIndex(x => x.UserId).IsUnique();

            });

            modelBuilder.Entity<Category>(entity =>
            {
                entity.ToTable("categories");
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Name).IsRequired().HasMaxLength(100);
                entity.Property(x => x.slug).IsRequired().HasMaxLength(150);
                
                //Relation (Self - Reference)
                entity.HasOne(x => x.Parent)
                .WithMany(p => p.Children)
                .HasForeignKey(x => x.ParentId)
                .OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(x => x.ParentId);

                entity.HasData(new Category
                    {
                        Id = 1,
                        Name = "electronics",
                        slug = "electronics",
                        ParentId = null,
                        isActive = true
                    },
                    new Category
                    {
                        Id = 2,
                        Name = "fashion",
                        slug = "fashion",
                        ParentId = null,
                        isActive = true
                    }
                );
            });

            modelBuilder.Entity<Product>(entity => {
                entity.ToTable("products");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Name).IsRequired().HasMaxLength(200);
                entity.Property(x => x.Description).HasMaxLength(4000);
                entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
                entity.Property(x => x.CreatedAt).HasColumnType("timestamp without time zone");

                //Relation(Product to Category => One to Many)
                entity.HasOne(x => x.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasIndex(x => x.CategoryId);
                
                //Relation(Product to Vendor => One to Many)
                entity.HasOne(x => x.Vendor)
                .WithMany(v => v.Products)
                .HasForeignKey(x => x.VendorId)
                .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasIndex(x => x.VendorId);
                
            });

            modelBuilder.Entity<ProductImage>(entity => {
                entity.ToTable("product_images");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.ImageUrl).IsRequired().HasMaxLength(500);
                entity.Property(x => x.ImageOrder).IsRequired();

                //Relation(ProductVariant to ProductImages => One to Many)
                entity.HasOne(x => x.ProductVariant)
                .WithMany(pv => pv.VariantImages)
                .HasForeignKey(x => x.VariantId)
                .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasIndex(x => x.VariantId);
                
            });

            modelBuilder.Entity<ReviewImage>(entity =>
            {
                entity.ToTable("review_images");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.ImageUrl).IsRequired().HasMaxLength(500);
                entity.Property(x => x.ImageOrder).IsRequired();
                entity.Property(x => x.ReviewId).IsRequired();

                //Relation(ReviewImage to Review => One to Many)
                entity.HasOne(x => x.Review)
                .WithMany(r => r.ReviewImages)
                .HasForeignKey(x => x.ReviewId)
                .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => x.ReviewId);
            });

            modelBuilder.Entity<ProductVariant>(entity => {
                entity.ToTable("product_variants");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Price);
                entity.Property(x => x.StockQty).IsRequired();
                entity.Property(x => x.AvailableValues)
                        .HasColumnType("jsonb")
                        .HasConversion(
                list => JsonSerializer.Serialize(list, (JsonSerializerOptions?)null),
                json => JsonSerializer.Deserialize<Dictionary<string, string>>(json,
                     (JsonSerializerOptions?)null) ?? new Dictionary<string, string>()
                );

                //Relation(ProductVariant to Product => One to Many)
                entity.HasOne(x => x.Product)
                .WithMany(p => p.Variants)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasIndex(x => x.ProductId);
                
            });

            modelBuilder.Entity<Cart>(entity => {
                entity.ToTable("carts");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.UpdatedAt).HasColumnType("timestamp without time zone");

                //Relation(Cart to User => one to one)
                entity.HasOne(x => x.User)
                .WithOne(u => u.Cart)
                .HasForeignKey<Cart>(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => x.UserId).IsUnique();
            });

            modelBuilder.Entity<CartItem>(entity => {
                entity.ToTable("cart_items");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Quantity).IsRequired();
                
                //Relation(Cart to CartItems => One to Many)
                entity.HasOne(x => x.Cart)
                .WithMany(c => c.Items)
                .HasForeignKey(x => x.CartId)
                .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => x.CartId);

                //Relation(ProductVariant to CartItems => One to Many)
                entity.HasOne(x => x.Variant)
                .WithMany(p => p.CartItems)
                .HasForeignKey(x => x.VariantId)
                .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => x.VariantId);

            });

            modelBuilder.Entity<Discount>(entity => {
                entity.ToTable("discounts");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Code).IsRequired().HasMaxLength(20);
                entity.HasIndex(x => x.Code).IsUnique();
                entity.Property(x => x.Type).HasConversion<string>().HasMaxLength(20);
                entity.Property(x => x.Scope).HasConversion<string>().HasMaxLength(20);               
                entity.Property(x => x.Value);
                entity.Property(x => x.IsActive).IsRequired();
                entity.Property(x => x.ExpiresAt).HasColumnType("timestamp without time zone");
            });

            modelBuilder.Entity<Order>(entity =>{
                entity.ToTable("orders");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.UserId).IsRequired();
                entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);
                entity.Property(x => x.OrderPaymentStatus).HasConversion<string>().HasMaxLength(20);
                entity.Property(x => x.StripePaymentIntentId).HasMaxLength(100);
                entity.HasIndex(x => x.StripePaymentIntentId).IsUnique().HasFilter("\"StripePaymentIntentId\" IS NOT NULL");
                entity.Property(x => x.Subtotal);
                entity.Property(x => x.DiscountAmount);
                entity.Property(x => x.TaxAmount);
                entity.Property(x => x.ShippingAmount);
                entity.Property(x => x.Total);
                entity.Property(x => x.PlacedAt).HasColumnType("timestamp without time zone");

                //Relation(Order to User => One to Many)
                entity.HasOne(x => x.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => x.UserId);

                //Relation(Order to Discount => One to Many)
                entity.HasOne(x => x.Discount)
                .WithMany(d => d.Orders)
                .HasForeignKey(x => x.DiscountId)
                .OnDelete(DeleteBehavior.Restrict);

                //Relation(Order to Payment => One to One)
                entity.HasOne(x => x.Payment)
                .WithOne(p => p.Order)
                .HasForeignKey<Order>(p => p.PaymentId)
                .OnDelete(DeleteBehavior.Restrict);

            });

            modelBuilder.Entity<OrderItem>(entity => {
                entity.ToTable("order_items");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.OrderId).IsRequired();
                entity.Property(x => x.VariantId).IsRequired();
                entity.Property(x => x.VendorId).IsRequired();
                entity.Property(x => x.Quantity).IsRequired();
                entity.Property(x => x.UnitPrice);
                
                //Relation(Order to OrderItems => One to Many)
                entity.HasOne(x => x.Order)
                .WithMany(o => o.Items)
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasIndex(x => x.OrderId);
                
                //Relation(ProductVariant to OrderItem => One to Many)
                entity.HasOne(x => x.Variant)
                .WithMany(pv => pv.OrderItems)
                .HasForeignKey(x => x.VariantId)
                .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasIndex(x => x.VariantId);

                //Relation(Shipment to OrderItem => One to Many)
                entity.HasOne(x => x.Shipment)
                .WithMany(s => s.OrderItems)
                .HasForeignKey(x => x.ShipmentId)
                .OnDelete(DeleteBehavior.Restrict);

                //Relation(Vendor to OrderItem => One to Many)
                entity.HasOne(x => x.Vendor)
                .WithMany(v => v.OrderItems)
                .HasForeignKey(x => x.VendorId)
                .OnDelete(DeleteBehavior.Restrict);

            });

            modelBuilder.Entity<Shipment>(entity => {
                entity.ToTable("shipments");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.TrackingNumber).IsRequired();
                entity.Property(x => x.UserAddressId).IsRequired();
                entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
                entity.Property(x => x.EstimatedFullfillement).HasColumnType("date");
                entity.Property(x => x.ShippedAt).HasColumnType("timestamp without time zone");
                entity.Property(x => x.FulfilledAt).HasColumnType("timestamp without time zone");

                
                //Relation(UserAdress to shipment => One to Many)
                entity.HasOne(x => x.UserAddress)
                .WithMany(u => u.Shipments)
                .HasForeignKey(x => x.UserAddressId)
                .OnDelete(DeleteBehavior.Restrict);

            });

            modelBuilder.Entity<Payment>(entity =>
            {
                entity.ToTable("payments");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.TransactionId).IsRequired();
                entity.Property(x => x.Amount);
                entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
                entity.Property(x => x.PaidAt).HasColumnType("timestamp without time zone");

            });

            modelBuilder.Entity<Return>(entity => {
                entity.ToTable("returns");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.OrderId).IsRequired();
                entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
                entity.Property(x => x.RequestedAt).HasColumnType("timestamp without time zone");
                entity.Property(x => x.CompletedAt).HasColumnType("timestamp without time zone");
                entity.Property(x => x.ReturnNumber).HasMaxLength(20);
                entity.Property(x => x.TotalRefundAmount);
                entity.Property(x => x.Reason).HasMaxLength(500);
                
                //Relation(Order to Return => One to One)
                entity.HasOne(x => x.Order)
                .WithMany(o => o.Returns)
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

                //Relation(Return to Shipment => one to one)
                entity.HasOne(x => x.Shipment)
                .WithOne(s => s.Return)
                .HasForeignKey<Return>(s => s.ShipmentId)
                .OnDelete(DeleteBehavior.Restrict);

                //Relation(Return to Payment => one to one)
                entity.HasOne(x => x.Payment)
                .WithOne(p => p.Return)
                .HasForeignKey<Return>(p => p.PaymentId)
                .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<ReturnItem>(entity => {
                entity.ToTable("return_items");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.ReturnId).IsRequired();
                entity.Property(x => x.OrderItemId).IsRequired();
                entity.Property(x => x.Quantity).IsRequired();
                entity.Property(x => x.Reason).IsRequired().HasMaxLength(500);
                entity.Property(x => x.RefundAmount);

                
                //Relation(Return to ReturnItem => One to Many)
                entity.HasOne(x => x.Return)
                .WithMany(r => r.Items)
                .HasForeignKey(x => x.ReturnId)
                .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasIndex(x => x.ReturnId);

                //Relation(OrderItem to ReturnItem => One to Many)
                entity.HasOne(x => x.OrderItem)
                .WithMany(y => y.ReturnItems)
                .HasForeignKey(x => x.OrderItemId)
                .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasIndex(x => x.OrderItemId);
            });

            modelBuilder.Entity<Review>(enity => {
                enity.ToTable("reviews");
                enity.HasKey(x => x.Id);
                enity.Property(x => x.UserId).IsRequired();
                enity.Property(x => x.ProductId).IsRequired();
                enity.Property(x => x.OrderId).IsRequired();
                enity.Property(x => x.Rating).IsRequired();
                enity.Property(x => x.Title).IsRequired().HasMaxLength(100);
                enity.Property(x => x.Body).HasMaxLength(500);
                enity.Property(x => x.CreatedAt).IsRequired().HasColumnType("timestamp without time zone");

                //Relation(User to Review => One to Many)
                enity.HasOne(x => x.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
                
                enity.HasIndex(x => x.UserId);

                //Relation(Product to Review => One to Many)
                enity.HasOne(x => x.Product)
                .WithMany(p => p.Reviews)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
                
                enity.HasIndex(x => x.ProductId);

                //Relation(Order to Review => One to Many)
                enity.HasOne(x => x.Order)
                .WithMany(o => o.Reviews)
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Restrict);
                
                enity.HasIndex(x => x.OrderId);
            });

            modelBuilder.Entity<Wishlist>(entity => {
                entity.ToTable("wishlists");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.UserId).IsRequired();

                //Relation(User to Wishlist => One to Many)
                entity.HasOne(x => x.User)
                .WithMany(u => u.Wishlists)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasIndex(x => x.UserId);

               //Relation(WishList to WishListItem => One to Many)
                entity.HasMany(x => x.Items)
                .WithOne(wi => wi.Wishlist)
                .HasForeignKey(wi => wi.WishlistId)
                .OnDelete(DeleteBehavior.Restrict);
        
            });

            modelBuilder.Entity<WishlistItem>(enity => {
                enity.ToTable("wishlist_items");
                enity.HasKey(x => x.Id);
                enity.Property(x => x.WishlistId).IsRequired();
                enity.Property(x => x.VariantId).IsRequired();
                enity.Property(x => x.AddedAt).IsRequired().HasColumnType("timestamp without time zone");

                //Relation(Wishlist to WishlistItem => One to Many)
                enity.HasOne(x => x.Wishlist)
                .WithMany(w => w.Items)
                .HasForeignKey(x => x.WishlistId)
                .OnDelete(DeleteBehavior.Restrict);
                
                enity.HasIndex(x => x.WishlistId);

                //Relation(ProductVariant to WishlistItem => One to Many)
                enity.HasOne(x => x.Variant)
                .WithMany(p => p.WishlistItems)
                .HasForeignKey(x => x.VariantId)
                .OnDelete(DeleteBehavior.Restrict);
                
                enity.HasIndex(x => x.VariantId);
            });

            modelBuilder.Entity<Notification>(enity => {
                enity.ToTable("notifications");
                enity.HasKey(x => x.Id);
                enity.Property(x => x.UserId).IsRequired();
                enity.Property(x => x.Title).IsRequired().HasMaxLength(100);
                enity.Property(x => x.Message).IsRequired().HasMaxLength(500);
                enity.Property(x => x.Type).HasConversion<string>().IsRequired();
                enity.Property(x => x.Level).HasConversion<string>().IsRequired();
                enity.Property(x => x.IsRead).HasDefaultValueSql("false");
                enity.Property(x => x.CreatedAt).IsRequired().HasColumnType("timestamp without time zone");
                
                //Relation(User to Notification => One to Many)
                enity.HasOne(x => x.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
                
                enity.HasIndex(x => x.UserId);
            });

            modelBuilder.Entity<VendorSettlement>(entity =>
            {
               entity.ToTable("vendor_settlements");
               entity.HasKey(x => x.Id);
               entity.Property(x => x.VendorId).IsRequired();
               entity.Property(x => x.OrderId).IsRequired();
               entity.Property(x => x.GrossAmount).IsRequired();
               entity.Property(x => x.ShippingAmount);
               entity.Property(x => x.VendorDiscountAmount);
               entity.Property(x => x.PlatformCommissionAmount).IsRequired();
               entity.Property(x => x.NetPayoutAmount);
               entity.Property(x => x.TransactionReference).HasMaxLength(100);
               entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
               entity.Property(x => x.SettledAt).HasColumnType("timestamp without time zone");

               // Relation(Vendor to VendorSettlement => One to Many)
               entity.HasOne(x => x.Vendor)
               .WithMany(v => v.VendorSettlement)
               .HasForeignKey(x => x.VendorId)
               .OnDelete(DeleteBehavior.Restrict);

               // Relation(Order to VendorSettlement => One to Many)
               entity.HasOne(x => x.Order)
               .WithMany(o => o.VendorSettlement)
               .HasForeignKey(x => x.OrderId)
               .OnDelete(DeleteBehavior.Restrict);

               entity.HasIndex(x => new { x.VendorId, x.OrderId });
            });

            // StockReservation: soft-lock table for PendingPayment orders
            modelBuilder.Entity<StockReservation>(entity =>
            {
               entity.ToTable("stock_reservations");
               entity.HasKey(x => x.Id);
               entity.Property(x => x.OrderId).IsRequired();
               entity.Property(x => x.VariantId).IsRequired();
               entity.Property(x => x.Quantity).IsRequired();
               entity.Property(x => x.IsReleased).HasDefaultValueSql("false");
               entity.Property(x => x.ReservedAt).HasColumnType("timestamp without time zone");
               entity.Property(x => x.ReleasedAt).HasColumnType("timestamp without time zone");

               // Relation(Order to StockReservation => One to Many)
               entity.HasOne(x => x.Order)
               .WithMany()
               .HasForeignKey(x => x.OrderId)
               .OnDelete(DeleteBehavior.Restrict);

               // Relation(ProductVariant to StockReservation => One to Many)
               entity.HasOne(x => x.Variant)
               .WithMany()
               .HasForeignKey(x => x.VariantId)
               .OnDelete(DeleteBehavior.Restrict);

               entity.HasIndex(x => new { x.OrderId, x.VariantId });
               entity.HasIndex(x => x.IsReleased);
            });
            
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }


        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
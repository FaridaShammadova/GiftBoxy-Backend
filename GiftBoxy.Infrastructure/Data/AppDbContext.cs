using GiftBoxy.Domain.Entities;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GiftBoxy.Infrastructure.Data
{
    public class AppDbContext : IdentityDbContext<AppUser>, IDataProtectionKeyContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }


        public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }

        public DbSet<Product> Products { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Wishlist> Wishlists { get; set; }
        public DbSet<WishlistItem> WishlistItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<ProductQuestion> ProductQuestions { get; set; }
        public DbSet<Coupon> Coupons { get; set; }
        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<SellerProfile> SellerProfiles { get; set; }
        public DbSet<SellerCategory> SellerCategories { get; set; }
        public DbSet<ProductRecipientTag> ProductRecipientTags { get; set; }
        public DbSet<ProductOccasionTag> ProductOccasionTags { get; set; }
        public DbSet<ProductInterestTag> ProductInterestTags { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);


            // Enums
            builder.Entity<AppUser>()
                 .Property(x => x.Role)
                 .HasConversion<string>();

            builder.Entity<Order>()
                .Property(x => x.Status)
                .HasConversion<string>();

            builder.Entity<Order>()
                .Property(x => x.PaymentStatus)
                .HasConversion<string>();

            builder.Entity<Order>()
                .Property(x => x.PaymentMethod)
                .HasConversion<string>();


            // Decimals
            builder.Entity<Product>()
                .Property(x => x.Price)
                .HasColumnType("decimal(18,2)");

            builder.Entity<Product>()
                .Property(x => x.OldPrice)
                .HasColumnType("decimal(18,2)");

            builder.Entity<Order>()
                .Property(x => x.TotalPrice)
                .HasColumnType("decimal(18,2)");

            builder.Entity<OrderItem>()
                .Property(x => x.Price)
                .HasColumnType("decimal(18,2)");

            builder.Entity<Coupon>()
                .Property(x => x.DiscountPercent)
                .HasColumnType("decimal(5,2)");

            builder.Entity<Coupon>()
                .Property(x => x.MinimumAmount)
                .HasColumnType("decimal(18,2)");


            // SellerProfile
            builder.Entity<SellerProfile>()
                .HasOne(x => x.User)
                .WithOne(x => x.SellerProfile)
                .HasForeignKey<SellerProfile>(x => x.UserId);


            //Coupon
            builder.Entity<Coupon>()
                .HasOne(c => c.Seller)
                .WithMany()
                .HasForeignKey(c => c.SellerId)
                .OnDelete(DeleteBehavior.Restrict);


            // Message
            builder.Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany(u => u.SentMessages)
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Message>()
                .HasOne(m => m.Conversation)
                .WithMany(c => c.Messages)
                .HasForeignKey(m => m.ConversationId)
                .OnDelete(DeleteBehavior.Restrict);


            // Product
            builder.Entity<Product>()
               .HasOne(p => p.Category)
               .WithMany(c => c.Products)
               .HasForeignKey(p => p.CategoryId);

            builder.Entity<ProductImage>()
                .HasOne(pi => pi.Product)
                .WithMany(p => p.Images)
                .HasForeignKey(pi => pi.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany(p => p.OrderItems)
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Review>()
                .HasOne(r => r.Product)
                .WithMany(p => p.Reviews)
                .HasForeignKey(r => r.ProductId)
                .OnDelete(DeleteBehavior.NoAction);


            //Cart
            builder.Entity<CartItem>()
                .HasOne(ci => ci.Cart)
                .WithMany(c => c.CartItems)
                .HasForeignKey(ci => ci.CartId);

            builder.Entity<CartItem>()
                .HasOne(ci => ci.Product)
                .WithMany(p => p.CartItems)
                .HasForeignKey(ci => ci.ProductId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Cart>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            //Wishlist
            builder.Entity<Wishlist>()
                .HasOne(w => w.User)
                .WithMany(u => u.Wishlist)
                .HasForeignKey(w => w.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<WishlistItem>()
                .HasOne(wi => wi.Wishlist)
                .WithMany(w => w.WishlistItems)
                .HasForeignKey(wi => wi.WishlistId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<WishlistItem>()
                .HasOne(wi => wi.Product)
                .WithMany()
                .HasForeignKey(wi => wi.ProductId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<WishlistItem>()
                .HasIndex(x => new { x.WishlistId, x.ProductId })
                .IsUnique();


            //Order
            builder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);


            //ProductQuestion
            builder.Entity<ProductQuestion>()
                .HasOne(q => q.Product)
                .WithMany(p => p.Questions)
                .HasForeignKey(q => q.ProductId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<ProductQuestion>()
                .HasOne(q => q.User)
                .WithMany(u => u.AskedQuestions)
                .HasForeignKey(q => q.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ProductQuestion>()
                .HasOne(q => q.Seller)
                .WithMany(u => u.ReceivedQuestions)
                .HasForeignKey(q => q.SellerId)
                .OnDelete(DeleteBehavior.Restrict);


            //Conversation
            builder.Entity<Conversation>()
                .HasOne(c => c.Buyer)
                .WithMany()
                .HasForeignKey(c => c.BuyerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Conversation>()
                .HasOne(c => c.Seller)
                .WithMany()
                .HasForeignKey(c => c.SellerId)
                .OnDelete(DeleteBehavior.Restrict);


            //SellerCategory
            builder.Entity<SellerCategory>()
                .HasOne(sc => sc.SellerProfile)
                .WithMany(sp => sp.SellerCategories)
                .HasForeignKey(sc => sc.SellerProfileId);

            builder.Entity<SellerCategory>()
                .HasOne(sc => sc.Category)
                .WithMany(c => c.SellerCategories)
                .HasForeignKey(sc => sc.CategoryId);


            // Unique constraints
            builder.Entity<CartItem>()
                .HasIndex(x => new { x.CartId, x.ProductId })
                .IsUnique();

            builder.Entity<Review>()
                .HasIndex(x => new { x.UserId, x.ProductId })
                .IsUnique();

            builder.Entity<Coupon>()
                .HasIndex(x => x.Code)
                .IsUnique();

            builder.Entity<SellerCategory>()
                .HasKey(sc => new { sc.SellerProfileId, sc.CategoryId });
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                if (entry.State == EntityState.Modified)
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}

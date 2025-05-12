using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RevampAuto.Models;

namespace RevampAuto.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Favorite> Favorites { get; set; }
        public DbSet<ContactMessage> ContactMessages { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // Set default schema for all tables
            builder.HasDefaultSchema("dbc");

            base.OnModelCreating(builder);

            // Explicitly map Identity tables
            builder.Entity<User>().ToTable("Users");
            builder.Entity<IdentityRole>().ToTable("Roles");
            builder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");
            builder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims");
            builder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins");
            builder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims");
            builder.Entity<IdentityUserToken<string>>().ToTable("UserTokens");

            // Configure decimal precision
            builder.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasColumnType("decimal(18,2)");

            builder.Entity<OrderItem>()
                .Property(oi => oi.UnitPrice)
                .HasColumnType("decimal(18,2)");

            builder.Entity<Product>()
                .Property(p => p.Price)
                .HasColumnType("decimal(18,2)");

            // Configure relationships
            builder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany()
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany()
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Order>()
                .HasMany(o => o.OrderItems)
                .WithOne(oi => oi.Order)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany()
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Favorite>()
                .HasOne(f => f.User)
                .WithMany()
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Favorite>()
                .HasOne(f => f.Product)
                .WithMany()
                .HasForeignKey(f => f.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Product>()
                .HasMany(p => p.Images)
                .WithOne(pi => pi.Product)
                .HasForeignKey(pi => pi.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Cart configurations
            builder.Entity<Cart>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Cart>()
                .HasMany(c => c.Items)
                .WithOne(ci => ci.Cart)
                .HasForeignKey(ci => ci.CartId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<CartItem>()
                .HasOne(ci => ci.Product)
                .WithMany()
                .HasForeignKey(ci => ci.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure indexes
            builder.Entity<Product>().HasIndex(p => p.CategoryId);
            builder.Entity<Order>().HasIndex(o => o.UserId);
            builder.Entity<Order>().HasIndex(o => o.Status);
            builder.Entity<OrderItem>().HasIndex(oi => oi.OrderId);
            builder.Entity<OrderItem>().HasIndex(oi => oi.ProductId);
            builder.Entity<Favorite>().HasIndex(f => f.UserId);
            builder.Entity<Favorite>().HasIndex(f => f.ProductId);

            // Cart indexes
            builder.Entity<Cart>().HasIndex(c => c.UserId).IsUnique();
            builder.Entity<CartItem>().HasIndex(ci => ci.CartId);
            builder.Entity<CartItem>().HasIndex(ci => ci.ProductId);
            builder.Entity<CartItem>().HasIndex(ci => new { ci.CartId, ci.ProductId }).IsUnique();

            // Configure default values
            builder.Entity<Order>()
                .Property(o => o.Status)
                .HasDefaultValue("Pending");

            builder.Entity<Order>()
                .Property(o => o.OrderDate)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Entity<ContactMessage>()
                .Property(c => c.SentAt)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Entity<ContactMessage>()
                .Property(c => c.IsRead)
                .HasDefaultValue(false);

            // Cart default values
            builder.Entity<Cart>()
                .Property(c => c.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Entity<CartItem>()
                .Property(ci => ci.AddedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            // Configure string lengths
            builder.Entity<Category>().Property(c => c.Name).HasMaxLength(100);
            builder.Entity<Product>().Property(p => p.Name).HasMaxLength(200);
            builder.Entity<ContactMessage>().Property(c => c.Name).HasMaxLength(100);
            builder.Entity<ContactMessage>().Property(c => c.Email).HasMaxLength(100);
            builder.Entity<ContactMessage>().Property(c => c.Message).HasMaxLength(500);
            builder.Entity<ProductImage>().Property(pi => pi.ImagePath).HasMaxLength(500);
            builder.Entity<Cart>().Property(c => c.UserId).HasMaxLength(450);
        }
    }
}
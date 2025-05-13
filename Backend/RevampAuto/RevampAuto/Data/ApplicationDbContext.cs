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
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<ShippingDetails> ShippingDetails { get; set; }
        public DbSet<Favorite> Favorites { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Discount> Discounts { get; set; }
        public DbSet<ContactMessage> ContactMessages { get; set; }
        public DbSet<Notification> Notifications { get; set; }
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

            builder.Entity<Discount>()
                .Property(d => d.Percentage)
                .HasColumnType("decimal(5,2)");

            // Configure relationships
            builder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany()
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Product>()
                .HasMany(p => p.Images)
                .WithOne(pi => pi.Product)
                .HasForeignKey(pi => pi.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

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

            builder.Entity<ShippingDetails>()
                .HasOne(sd => sd.Order)
                .WithOne()
                .HasForeignKey<ShippingDetails>(sd => sd.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

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

            builder.Entity<Review>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Review>()
                .HasOne(r => r.Product)
                .WithMany()
                .HasForeignKey(r => r.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Discount>()
                .HasOne(d => d.Category)
                .WithMany()
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Discount>()
                .HasOne(d => d.Product)
                .WithMany()
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

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
            builder.Entity<Product>().HasIndex(p => p.Name);

            builder.Entity<Order>().HasIndex(o => o.UserId);
            builder.Entity<Order>().HasIndex(o => o.Status);
            builder.Entity<Order>().HasIndex(o => o.OrderDate);

            builder.Entity<OrderItem>().HasIndex(oi => oi.OrderId);
            builder.Entity<OrderItem>().HasIndex(oi => oi.ProductId);

            builder.Entity<ShippingDetails>().HasIndex(sd => sd.OrderId).IsUnique();
            builder.Entity<ShippingDetails>().HasIndex(sd => sd.PostalCode);

            builder.Entity<Favorite>().HasIndex(f => f.UserId);
            builder.Entity<Favorite>().HasIndex(f => f.ProductId);
            builder.Entity<Favorite>().HasIndex(f => new { f.UserId, f.ProductId }).IsUnique();

            builder.Entity<Review>().HasIndex(r => r.UserId);
            builder.Entity<Review>().HasIndex(r => r.ProductId);
            builder.Entity<Review>().HasIndex(r => r.Rating);

            builder.Entity<Discount>().HasIndex(d => d.Code).IsUnique();
            builder.Entity<Discount>().HasIndex(d => d.CategoryId);
            builder.Entity<Discount>().HasIndex(d => d.ProductId);
            builder.Entity<Discount>().HasIndex(d => d.IsActive);
            builder.Entity<Discount>().HasIndex(d => d.StartDate);
            builder.Entity<Discount>().HasIndex(d => d.EndDate);

            builder.Entity<ContactMessage>().HasIndex(c => c.Email);
            builder.Entity<ContactMessage>().HasIndex(c => c.IsRead);

            builder.Entity<Notification>().HasIndex(n => n.UserId);
            builder.Entity<Notification>().HasIndex(n => n.IsRead);
            builder.Entity<Notification>().HasIndex(n => n.NotificationType);
            builder.Entity<Notification>().HasIndex(n => n.CreatedAt);

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

            builder.Entity<Review>()
                .Property(r => r.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Entity<Review>()
                .Property(r => r.Rating)
                .HasDefaultValue(5);

            builder.Entity<ContactMessage>()
                .Property(c => c.SentAt)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Entity<ContactMessage>()
                .Property(c => c.IsRead)
                .HasDefaultValue(false);

            builder.Entity<Notification>()
                .Property(n => n.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Entity<Notification>()
                .Property(n => n.IsRead)
                .HasDefaultValue(false);

            builder.Entity<Discount>()
                .Property(d => d.IsActive)
                .HasDefaultValue(true);

            builder.Entity<Discount>()
                .Property(d => d.CurrentUses)
                .HasDefaultValue(0);

            builder.Entity<ProductImage>()
                .Property(pi => pi.IsMainImage)
                .HasDefaultValue(false);

            builder.Entity<ProductImage>()
                .Property(pi => pi.UploadedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            // Cart default values
            builder.Entity<Cart>()
                .Property(c => c.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Entity<CartItem>()
                .Property(ci => ci.AddedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Entity<CartItem>()
                .Property(ci => ci.Quantity)
                .HasDefaultValue(1);

            // Configure string lengths
            builder.Entity<Category>().Property(c => c.Name).HasMaxLength(100);
            builder.Entity<Category>().Property(c => c.Description).HasMaxLength(500);

            builder.Entity<Product>().Property(p => p.Name).HasMaxLength(200);
            builder.Entity<Product>().Property(p => p.Description).HasMaxLength(2000);

            builder.Entity<ProductImage>().Property(pi => pi.ImagePath).HasMaxLength(500);

            builder.Entity<ShippingDetails>().Property(sd => sd.FullName).HasMaxLength(100);
            builder.Entity<ShippingDetails>().Property(sd => sd.AddressLine1).HasMaxLength(200);
            builder.Entity<ShippingDetails>().Property(sd => sd.AddressLine2).HasMaxLength(200);
            builder.Entity<ShippingDetails>().Property(sd => sd.City).HasMaxLength(100);
            builder.Entity<ShippingDetails>().Property(sd => sd.State).HasMaxLength(100);
            builder.Entity<ShippingDetails>().Property(sd => sd.PostalCode).HasMaxLength(20);
            builder.Entity<ShippingDetails>().Property(sd => sd.Country).HasMaxLength(100);
            builder.Entity<ShippingDetails>().Property(sd => sd.PhoneNumber).HasMaxLength(20);
            builder.Entity<ShippingDetails>().Property(sd => sd.TrackingNumber).HasMaxLength(100);
            builder.Entity<ShippingDetails>().Property(sd => sd.ShippingMethod).HasMaxLength(50);

            builder.Entity<ContactMessage>().Property(c => c.Name).HasMaxLength(100);
            builder.Entity<ContactMessage>().Property(c => c.Email).HasMaxLength(100);
            builder.Entity<ContactMessage>().Property(c => c.Message).HasMaxLength(500);

            builder.Entity<Notification>().Property(n => n.Title).HasMaxLength(200);
            builder.Entity<Notification>().Property(n => n.Message).HasMaxLength(1000);
            builder.Entity<Notification>().Property(n => n.NotificationType).HasMaxLength(50);

            builder.Entity<Review>().Property(r => r.Comment).HasMaxLength(1000);

            builder.Entity<Discount>().Property(d => d.Code).HasMaxLength(50);
            builder.Entity<Discount>().Property(d => d.Description).HasMaxLength(500);

            builder.Entity<User>().Property(u => u.FirstName).HasMaxLength(100);
            builder.Entity<User>().Property(u => u.LastName).HasMaxLength(100);
            builder.Entity<User>().Property(u => u.Address).HasMaxLength(500);
        }
    }
}
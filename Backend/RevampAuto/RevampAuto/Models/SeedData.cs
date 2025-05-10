using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RevampAuto.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RevampAuto.Data
{
    public static class SeedData
    {
        public static async Task Initialize(ApplicationDbContext context,
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger logger = null)
        {
            try
            {
                // Ensure database is created and migrated
                logger?.LogInformation("Applying pending migrations...");
                await context.Database.MigrateAsync();
                logger?.LogInformation("Migrations applied successfully");

                // Seed Roles
                await SeedRoles(roleManager, logger);

                // Seed Admin User
                await SeedAdminUser(userManager, logger);

                // Seed Sample Customer
                await SeedCustomerUser(userManager, logger);

                // Seed Sample Categories
                await SeedCategories(context, logger);

                // Seed Sample Products
                await SeedProducts(context, logger);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "An error occurred while seeding the database");
                throw; // Re-throw to fail startup if seeding fails
            }
        }

        private static async Task SeedRoles(RoleManager<IdentityRole> roleManager, ILogger logger)
        {
            logger?.LogInformation("Seeding roles...");
            string[] roleNames = { "Admin", "Customer", "Manager" }; // Added Manager role

            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    logger?.LogInformation($"Creating role: {roleName}");
                    var result = await roleManager.CreateAsync(new IdentityRole(roleName));

                    if (result.Succeeded)
                    {
                        logger?.LogInformation($"Successfully created role: {roleName}");
                    }
                    else
                    {
                        var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                        logger?.LogError($"Failed to create role {roleName}: {errors}");
                        throw new Exception($"Failed to create role {roleName}: {errors}");
                    }
                }
            }
        }

        private static async Task SeedAdminUser(UserManager<User> userManager, ILogger logger)
        {
            const string adminEmail = "admin@revampauto.com";
            const string adminPassword = "Admin@1234"; // Strong password

            logger?.LogInformation($"Seeding admin user: {adminEmail}");

            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new User
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "Admin",
                    LastName = "User",
                    EmailConfirmed = true, // Skip email confirmation for seeding
                    PhoneNumber = "+1234567890",
                    PhoneNumberConfirmed = true,
                    Address = "123 Admin Street",
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                var createResult = await userManager.CreateAsync(adminUser, adminPassword);
                if (createResult.Succeeded)
                {
                    logger?.LogInformation("Admin user created successfully");

                    // Assign admin role
                    var addToRoleResult = await userManager.AddToRoleAsync(adminUser, "Admin");
                    if (!addToRoleResult.Succeeded)
                    {
                        var errors = string.Join(", ", addToRoleResult.Errors.Select(e => e.Description));
                        logger?.LogError($"Failed to add admin role: {errors}");
                        throw new Exception($"Failed to add admin role: {errors}");
                    }
                    logger?.LogInformation("Admin role assigned successfully");
                }
                else
                {
                    var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                    logger?.LogError($"Failed to create admin user: {errors}");
                    throw new Exception($"Failed to create admin user: {errors}");
                }
            }
            else
            {
                logger?.LogInformation("Admin user already exists");
            }
        }

        private static async Task SeedCustomerUser(UserManager<User> userManager, ILogger logger)
        {
            const string customerEmail = "customer@revampauto.com";
            const string customerPassword = "Customer@1234"; // Strong password

            logger?.LogInformation($"Seeding customer user: {customerEmail}");

            var customerUser = await userManager.FindByEmailAsync(customerEmail);
            if (customerUser == null)
            {
                customerUser = new User
                {
                    UserName = customerEmail,
                    Email = customerEmail,
                    FirstName = "John",
                    LastName = "Doe",
                    EmailConfirmed = true,
                    PhoneNumber = "+1987654321",
                    PhoneNumberConfirmed = true,
                    Address = "456 Customer Avenue",
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                var createResult = await userManager.CreateAsync(customerUser, customerPassword);
                if (createResult.Succeeded)
                {
                    logger?.LogInformation("Customer user created successfully");

                    // Assign customer role
                    var addToRoleResult = await userManager.AddToRoleAsync(customerUser, "Customer");
                    if (!addToRoleResult.Succeeded)
                    {
                        var errors = string.Join(", ", addToRoleResult.Errors.Select(e => e.Description));
                        logger?.LogError($"Failed to add customer role: {errors}");
                    }
                    else
                    {
                        logger?.LogInformation("Customer role assigned successfully");
                    }
                }
                else
                {
                    var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                    logger?.LogError($"Failed to create customer user: {errors}");
                    // Don't throw for customer user - non-critical
                }
            }
            else
            {
                logger?.LogInformation("Customer user already exists");
            }
        }

        private static async Task SeedCategories(ApplicationDbContext context, ILogger logger)
        {
            logger?.LogInformation("Seeding categories...");

            if (!await context.Categories.AnyAsync())
            {
                var categories = new List<Category>
                {
                    new Category { Name = "Engine Parts", Description = "Various engine components", CreatedAt = DateTime.UtcNow },
                    new Category { Name = "Braking System", Description = "Brakes and related parts", CreatedAt = DateTime.UtcNow },
                    new Category { Name = "Suspension", Description = "Suspension components", CreatedAt = DateTime.UtcNow },
                    new Category { Name = "Electrical", Description = "Electrical system components", CreatedAt = DateTime.UtcNow },
                    new Category { Name = "Body & Interior", Description = "Body panels and interior parts", CreatedAt = DateTime.UtcNow }
                };

                await context.Categories.AddRangeAsync(categories);
                await context.SaveChangesAsync();
                logger?.LogInformation($"Seeded {categories.Count} categories");
            }
            else
            {
                logger?.LogInformation("Categories already exist - skipping seed");
            }
        }

        private static async Task SeedProducts(ApplicationDbContext context, ILogger logger)
        {
            logger?.LogInformation("Seeding products...");

            if (!await context.Products.AnyAsync())
            {
                var categories = await context.Categories.ToListAsync();
                if (!categories.Any())
                {
                    logger?.LogWarning("No categories found - skipping product seed");
                    return;
                }

                var products = new List<Product>
                {
                    new Product {
                        Name = "High Performance Air Filter",
                        Description = "Premium air filter for improved engine performance",
                        Price = 49.99m,
                        StockQuantity = 100,
                        CategoryId = categories.First(c => c.Name == "Engine Parts").Id,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Product {
                        Name = "Ceramic Brake Pads",
                        Description = "High-quality brake pads for superior stopping power",
                        Price = 89.99m,
                        StockQuantity = 75,
                        CategoryId = categories.First(c => c.Name == "Braking System").Id,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Product {
                        Name = "Sport Suspension Kit",
                        Description = "Complete suspension upgrade kit for improved handling",
                        Price = 499.99m,
                        StockQuantity = 25,
                        CategoryId = categories.First(c => c.Name == "Suspension").Id,
                        CreatedAt = DateTime.UtcNow
                    }
                };

                await context.Products.AddRangeAsync(products);
                await context.SaveChangesAsync();
                logger?.LogInformation($"Seeded {products.Count} products");
            }
            else
            {
                logger?.LogInformation("Products already exist - skipping seed");
            }
        }
    }
}
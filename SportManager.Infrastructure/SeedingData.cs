using Microsoft.EntityFrameworkCore;
using SportManager.Domain.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace SportManager.Infrastructure
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            var utcNow = DateTime.UtcNow;

            // Kiểm tra và thêm Roles nếu chưa có
            if (!context.Roles.Any())
            {
                var adminRoleId = Guid.NewGuid();
                var customerRoleId = Guid.NewGuid();

                await context.Roles.AddRangeAsync(
                    new Role { Id = adminRoleId, Name = "Admin", CreatedAt = utcNow },
                    new Role { Id = customerRoleId, Name = "User", CreatedAt = utcNow }
                );

                await context.SaveChangesAsync();
            }

            // Kiểm tra và thêm Users nếu chưa có
            if (!context.Users.Any(u => u.Username == "admin" || u.Email == "admin@sport.com"))
            {
                var adminUserId = Guid.NewGuid();
                var userID = Guid.NewGuid();

                await context.Users.AddRangeAsync(
                    new User { Id = userID, Username = "user123123", Email = "user123123@sport.com", PasswordHash = "hNvv6vqeUu+GJrpxPYTNKxAHi9Ur9dn3eZTBWVcywSKQ8oJKSffP3/H6z6lE+Bwo8vQ3lwVWFP4B7Hz5lwPYYw==", CreatedAt = utcNow },
                    new User { Id = adminUserId, Username = "admin", Email = "admin@sport.com", PasswordHash = "gYel5+9XJPXZg9LGITNHKj4aPIEn80YnWnYSvp6KbGdS2ckW7reLTBAisOhyWuZ/Q3tEX5hUDYnyv5Nu80quuA==", CreatedAt = utcNow }
                );

                await context.SaveChangesAsync();

                // Sau khi thêm Users, tạo mối quan hệ UserRole
                var adminRoleId = context.Roles.First(r => r.Name == "Admin").Id;
                var customerRoleId = context.Roles.First(r => r.Name == "User").Id;

                if (!context.UserRoles.Any(ur => ur.UserId == adminUserId && ur.RoleId == adminRoleId))
                {
                    await context.UserRoles.AddAsync(new UserRole { Id = Guid.NewGuid(), UserId = adminUserId, RoleId = adminRoleId, CreatedAt = utcNow });
                }

                if (!context.UserRoles.Any(ur => ur.UserId == userID && ur.RoleId == customerRoleId))
                {
                    await context.UserRoles.AddAsync(new UserRole { Id = Guid.NewGuid(), UserId = userID, RoleId = customerRoleId, CreatedAt = utcNow });
                }

                await context.SaveChangesAsync();
            }

            // Kiểm tra và thêm Customers nếu chưa có
            var customerUserId = context.Users.FirstOrDefault(u => u.Username == "user123123")?.Id;
            if (customerUserId != null && !context.Customers.Any(c => c.UserId == customerUserId))
            {
                var customerId = Guid.NewGuid();

                await context.Customers.AddAsync(
                    new Customer { Id = customerId, UserId = customerUserId.Value, Address = "Phú Đô, Nam Từ Liêm, Hà Nội", Gender = Gender.Male, Phone = "080080", CreatedAt = utcNow }
                );

                await context.SaveChangesAsync();
            }

            // Kiểm tra và thêm Brands nếu chưa có
            if (!context.Brands.Any())
            {
                var yonexId = Guid.NewGuid();
                var liningId = Guid.NewGuid();

                await context.Brands.AddRangeAsync(
                    new Brand { Id = yonexId, Name = "Yonex", Slug = "yonex", Country = "Japan", CountryId = "111", LogoUrl = "", Website = "", City = "Tokyo", IsActive = true, CreatedAt = utcNow },
                    new Brand { Id = liningId, Name = "Lining", Slug = "lining", Country = "China", City = "Beijing", CountryId = "1112", LogoUrl = "", Website = "", IsActive = true, CreatedAt = utcNow }
                );

                await context.SaveChangesAsync();
            }

            // Kiểm tra và thêm Categories nếu chưa có
            if (!context.Categories.Any())
            {
                var categoryRacketId = Guid.NewGuid();
                var categoryShoesId = Guid.NewGuid();

                await context.Categories.AddRangeAsync(
                    new Category { Id = categoryRacketId, Name = "Vợt cầu lông", Logo = "rackets.png", CreatedAt = utcNow },
                    new Category { Id = categoryShoesId, Name = "Giày cầu lông", Logo = "shoes.png", CreatedAt = utcNow }
                );

                await context.SaveChangesAsync();
            }

            // Kiểm tra và thêm Suppliers nếu chưa có
            if (!context.Suppliers.Any())
            {
                var supplier1Id = Guid.NewGuid();
                var supplier2Id = Guid.NewGuid();

                await context.Suppliers.AddRangeAsync(
                    new Supplier { Id = supplier1Id, Name = "Yonex Việt Nam", ContactPhone = "0123456789", Country = "Vietnam", Phone = "91283123123", ContactEmail = "contact1@gmail.com", IsActive = true, CreatedAt = utcNow },
                    new Supplier { Id = supplier2Id, Name = "Lining Việt Nam", ContactPhone = "0987654321", Country = "Vietnam", Phone = "91280123123", ContactEmail = "contact2@gmail.com", IsActive = true, CreatedAt = utcNow }
                );

                await context.SaveChangesAsync();
            }

            // Kiểm tra và thêm Products nếu chưa có
            if (!context.Products.Any(p => p.Name == "Vợt Yonex Astrox 99"))
            {
                var yonexId = context.Brands.FirstOrDefault(b => b.Name == "Yonex")?.Id;
                var supplier1Id = context.Suppliers.FirstOrDefault(s => s.Name == "Yonex Việt Nam")?.Id;
                var productId = Guid.NewGuid();

                if (yonexId != null && supplier1Id != null)
                {
                    await context.Products.AddAsync(new Product
                    {
                        Id = productId,
                        Name = "Vợt Yonex Astrox 99",
                        BrandId = yonexId.Value,
                        SupplierId = supplier1Id.Value,
                        CreatedAt = utcNow,
                        Images = Array.Empty<string>()
                    });

                    await context.SaveChangesAsync(); // Lưu để dùng ProductId ngay sau
                }
            }

            // Kiểm tra và thêm ProductVariants nếu chưa có
            var product = await context.Products.FirstOrDefaultAsync(p => p.Name == "Vợt Yonex Astrox 99");
            if (product != null && !context.ProductVariants.Any(pv => pv.ProductId == product.Id))
            {
                await context.ProductVariants.AddAsync(new ProductVariant
                {
                    Id = Guid.NewGuid(),
                    ProductId = product.Id,
                    Name = "Astrox 99 Pro Đỏ",
                    Color = "Đỏ",
                    Price = 2900000,
                    StockQuantity = 20,
                    SKU = "SKU",
                    Size = "10",
                    Unit = "10",
                    CreatedAt = utcNow,
                });

                await context.SaveChangesAsync();
            }

            // Kiểm tra và thêm ProductCategories nếu chưa có
            var category = context.Categories.FirstOrDefault(c => c.Name == "Vợt cầu lông");
            if (product != null && category != null && !context.ProductCategories.Any(pc => pc.ProductId == product.Id && pc.CategoryId == category.Id))
            {
                await context.ProductCategories.AddAsync(new ProductCategory
                {
                    Id = Guid.NewGuid(),
                    ProductId = product.Id,
                    CategoryId = category.Id,
                    CreatedAt = utcNow
                });

                await context.SaveChangesAsync();
            }
        }
    }
}

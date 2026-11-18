using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartRetail.Domain.Entities;
using SmartRetail.Infrastructure.Persistence.Contexts;

namespace SmartRetail.Infrastructure.Persistence
{
    public static class DbInitializer
    {
        public static async Task SeedUsersAsync(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            using var scope = serviceProvider.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // 1. Ensure Roles
            string[] roles = ["SuperAdmin", "Owner"];
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // 2. SuperAdmin
            var superAdminEmail = configuration["UsersData:SuperAdmin:Email"];
            var superAdminPass = configuration["UsersData:SuperAdmin:Password"];
            if (!string.IsNullOrEmpty(superAdminEmail) && !string.IsNullOrEmpty(superAdminPass))
            {
                var admin = await userManager.FindByEmailAsync(superAdminEmail);
                if (admin == null)
                {
                    admin = new User
                    {
                        UserName = superAdminEmail.Split('@')[0],
                        Email = superAdminEmail,
                        EmailConfirmed = true,
                        IsActive = true
                    };
                    await userManager.CreateAsync(admin, superAdminPass);
                    await userManager.AddToRoleAsync(admin, "SuperAdmin");
                }
                else
                {
                    // Update password to match appsettings.json if needed
                    var token = await userManager.GeneratePasswordResetTokenAsync(admin);
                    await userManager.ResetPasswordAsync(admin, token, superAdminPass);
                }
            }

            // 3. Owner
            var ownerEmail = configuration["UsersData:Owner:Email"];
            var ownerPass = configuration["UsersData:Owner:Password"];
            if (!string.IsNullOrEmpty(ownerEmail) && !string.IsNullOrEmpty(ownerPass))
            {
                var owner = await userManager.FindByEmailAsync(ownerEmail);
                if (owner == null)
                {
                    owner = new User
                    {
                        UserName = ownerEmail.Split('@')[0],
                        Email = ownerEmail,
                        EmailConfirmed = true,
                        IsActive = true
                    };
                    await userManager.CreateAsync(owner, ownerPass);
                    await userManager.AddToRoleAsync(owner, "Owner");
                }
                else
                {
                    // Update password to match appsettings.json
                    var token = await userManager.GeneratePasswordResetTokenAsync(owner);
                    await userManager.ResetPasswordAsync(owner, token, ownerPass);
                }

                // Update Owner profile to Egyptian Arabic localization
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var ownerUser = await dbContext.Users.Include(u => u.Address).FirstOrDefaultAsync(u => u.Id == owner.Id);
                if (ownerUser != null)
                {
                    ownerUser.FirstName = "زياد";
                    ownerUser.LastName = "أبوعنتر";
                    ownerUser.PhoneNumber = "+201000000000";
                    if (ownerUser.Address == null)
                    {
                        ownerUser.Address = new Address();
                    }
                    ownerUser.Address.Street = "شارع 9، المعادي";
                    ownerUser.Address.City = "القاهرة";
                    ownerUser.Address.Government = "القاهرة";
                    ownerUser.Address.Country = "مصر";
                    ownerUser.Address.ZipCode = "11728";
                    await dbContext.SaveChangesAsync();
                }

                // 4. Seed Default Store for Owner if none exists
                var store = dbContext.Stores.FirstOrDefault(s => s.OwnerId == owner.Id);
                if (store == null)
                {
                    store = new Store
                    {
                        Name = "سوبرماركت النخيل الذكي",
                        Address = "شارع 9، المعادي",
                        City = "القاهرة",
                        Latitude = "470", // Entrance X
                        Longitude = "520", // Entrance Y
                        IsActive = true,
                        OwnerId = owner.Id,
                        CreatedAt = DateTime.UtcNow
                    };
                    dbContext.Stores.Add(store);
                    await dbContext.SaveChangesAsync();
                }

                // Ensure store has the standard supermarket sections matching the interactive map
                var existingCount = dbContext.Categories.Count(c => c.StoreId == store.Id && c.ParentCategoryId == null);
                if (existingCount <= 1)
                {
                    // Definition of realistic sections matching reference map (Images 3 & 4)
                    var sections = new[]
                    {
                        new {
                            Name = "اللحوم",
                            Color = "#8b251e",
                            X = 375, Y = 30,
                            Subs = new[] { "دواجن", "لحوم حمراء", "أسماك", "مصنّعات" }
                        },
                        new {
                            Name = "خضار وفاكهة",
                            Color = "#1b8a2e",
                            X = 670, Y = 30,
                            Subs = new[] { "خضروات طازجة", "فواكه موسمية", "ورقيات وأعشاب", "فواكه مجففة" }
                        },
                        new {
                            Name = "الألبان",
                            Color = "#1c6282",
                            X = 80, Y = 30,
                            Subs = new[] { "حليب طازج", "أجبان متنوعة", "زبادي ورائب", "زبدة وقشطة" }
                        },
                        new {
                            Name = "المشروبات",
                            Color = "#531c7d",
                            X = 670, Y = 220,
                            Subs = new[] { "عصائر طبيعية", "مياه معدنية", "مشروبات غازية", "شاي وقهوة" }
                        },
                        new {
                            Name = "المخبوزات",
                            Color = "#915a1a",
                            X = 80, Y = 220,
                            Subs = new[] { "خبز بلدي وشامي", "توست وفينو", "معجنات وفطائر" }
                        },
                        new {
                            Name = "البقالة الجافة",
                            Color = "#711b74",
                            X = 670, Y = 410,
                            Subs = new[] { "أرز ومكرونة", "زيوت وسمن", "بقوليات وحبوب", "صلصات وتوابل" }
                        },
                        new {
                            Name = "المنظفات",
                            Color = "#1c7c77",
                            X = 80, Y = 410,
                            Subs = new[] { "منظفات ملابس", "مطهرات منزلية", "أدوات نظافة" }
                        }
                    };

                    foreach (var s in sections)
                    {
                        var parentCat = dbContext.Categories.FirstOrDefault(c => c.StoreId == store.Id && c.Name == s.Name);
                        if (parentCat == null)
                        {
                            parentCat = new Category
                            {
                                Name = s.Name,
                                Description = $"قسم {s.Name} الرئيسي",
                                ColorHex = s.Color,
                                GridX = s.X,
                                GridY = s.Y,
                                StoreId = store.Id,
                                IsVisible = true,
                                CreatedAt = DateTime.UtcNow
                            };
                            dbContext.Categories.Add(parentCat);
                            await dbContext.SaveChangesAsync();
                        }
                        else
                        {
                            parentCat.ColorHex = s.Color;
                            parentCat.GridX = s.X;
                            parentCat.GridY = s.Y;
                            await dbContext.SaveChangesAsync();
                        }

                        // Add subcategories if missing
                        foreach (var subName in s.Subs)
                        {
                            if (!dbContext.Categories.Any(c => c.StoreId == store.Id && c.ParentCategoryId == parentCat.Id && c.Name == subName))
                            {
                                dbContext.Categories.Add(new Category
                                {
                                    Name = subName,
                                    Description = $"فئة فرعية تابعة لقسم {s.Name}",
                                    ParentCategoryId = parentCat.Id,
                                    ColorHex = s.Color,
                                    StoreId = store.Id,
                                    IsVisible = true,
                                    CreatedAt = DateTime.UtcNow
                                });
                            }
                        }
                        await dbContext.SaveChangesAsync();
                    }
                }
            }
        }
    }
}

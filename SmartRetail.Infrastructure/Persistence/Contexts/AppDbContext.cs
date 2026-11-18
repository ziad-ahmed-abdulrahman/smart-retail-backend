using System.Reflection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SmartRetail.Domain.Entities;

namespace SmartRetail.Infrastructure.Persistence.Contexts
{
    public class AppDbContext : IdentityDbContext<User>
    {
        private readonly IConfiguration _configuration;

        public AppDbContext(DbContextOptions<AppDbContext> options, IConfiguration configuration) : base(options)
        {
            _configuration = configuration;
        }

        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<Store> Stores { get; set; }
        public DbSet<Device> Devices { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Rename Identity tables
            modelBuilder.Entity<User>(b => b.ToTable("Users"));
            modelBuilder.Entity<IdentityRole>(b => b.ToTable("Roles"));
            modelBuilder.Entity<IdentityUserRole<string>>(b => b.ToTable("UserRoles"));
            modelBuilder.Entity<IdentityUserClaim<string>>(b => b.ToTable("UserClaims"));
            modelBuilder.Entity<IdentityRoleClaim<string>>(b => b.ToTable("RoleClaims"));
            modelBuilder.Entity<IdentityUserLogin<string>>(b => b.ToTable("UserLogins"));
            modelBuilder.Entity<IdentityUserToken<string>>(b => b.ToTable("UserTokens"));

            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            // Two exact roles: SuperAdmin & Owner
            var SUPER_ADMIN_ROLE_ID = "fab4fac1-c546-41de-aebc-a14da6895711";
            var OWNER_ROLE_ID = "e82d3fa1-7f9a-4e2b-911b-3f4a5c6d7e8f";

            var SUPER_ADMIN_ID = "5f2d9e8a-b1c4-4e7a-9d6b-3f2a1c5d8e90";
            var DEFAULT_OWNER_ID = "d3c2b1a0-9e8d-7c6b-5a4f-3e2d1c0b9a8f";

            var superAdminEmail = _configuration["UsersData:SuperAdmin:Email"] ?? "superadmin@smartretail.com";
            var ownerEmail = _configuration["UsersData:Owner:Email"] ?? "owner@smartretail.com";

            // 1. Roles Seed (SuperAdmin, Owner)
            modelBuilder.Entity<IdentityRole>().HasData(
                new IdentityRole { Id = SUPER_ADMIN_ROLE_ID, Name = "SuperAdmin", NormalizedName = "SUPERADMIN" },
                new IdentityRole { Id = OWNER_ROLE_ID, Name = "Owner", NormalizedName = "OWNER" }
            );

            // 2. SuperAdmin Seed (Software Admin)
            var superAdmin = new User
            {
                Id = SUPER_ADMIN_ID,
                UserName = superAdminEmail.Split('@')[0],
                NormalizedUserName = superAdminEmail.Split('@')[0].ToUpper(),
                Email = superAdminEmail,
                NormalizedEmail = superAdminEmail.ToUpper(),
                EmailConfirmed = true,
                IsActive = true,
                PasswordHash = "AQAAAAIAAYagAAAAEMx5hHZO1XxyP6MtdZLYZMbcelKvpp0ge//TpOjNnxCqmZTry+LUmWomMxuEOkNW4Q==",
                SecurityStamp = "STATIC_STAMP_SUPERADMIN_1",
                ConcurrencyStamp = "STATIC_CONCURRENCY_1"
            };

            // 3. Default Owner Seed (Store Owner)
            var owner = new User
            {
                Id = DEFAULT_OWNER_ID,
                UserName = ownerEmail.Split('@')[0],
                NormalizedUserName = ownerEmail.Split('@')[0].ToUpper(),
                Email = ownerEmail,
                NormalizedEmail = ownerEmail.ToUpper(),
                EmailConfirmed = true,
                IsActive = true,
                PasswordHash = "AQAAAAIAAYagAAAAEGHWBtftyg7MnOnpacIQHBJoOplazow9qTOD2rMfpzJISitLfRSsfnaQ7pZAoNacEg==",
                SecurityStamp = "STATIC_STAMP_OWNER_1",
                ConcurrencyStamp = "STATIC_CONCURRENCY_2"
            };

            modelBuilder.Entity<User>().HasData(superAdmin, owner);

            // 4. Map Roles
            modelBuilder.Entity<IdentityUserRole<string>>().HasData(
                new IdentityUserRole<string> { RoleId = SUPER_ADMIN_ROLE_ID, UserId = SUPER_ADMIN_ID },
                new IdentityUserRole<string> { RoleId = OWNER_ROLE_ID, UserId = DEFAULT_OWNER_ID }
            );
        }
    }
}

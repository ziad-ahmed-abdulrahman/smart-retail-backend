using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartRetail.Domain.Entities;

namespace SmartRetail.Infrastructure.Persistence.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.Property(u => u.FirstName).IsRequired(false).HasMaxLength(20);
            builder.Property(u => u.LastName).IsRequired(false).HasMaxLength(20);

            builder.HasOne(u => u.Address)
                   .WithOne(a => a.User)
                   .HasForeignKey<Address>(a => a.UserId);

            builder.HasMany(u => u.Stores)
                   .WithOne(s => s.Owner)
                   .HasForeignKey(s => s.OwnerId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class StoreConfiguration : IEntityTypeConfiguration<Store>
    {
        public void Configure(EntityTypeBuilder<Store> builder)
        {
            builder.HasKey(s => s.Id);
            builder.Property(s => s.Name).IsRequired().HasMaxLength(100);
            builder.Property(s => s.Address).HasMaxLength(250);
            builder.Property(s => s.City).HasMaxLength(50);

            builder.HasMany(s => s.Devices)
                   .WithOne(d => d.Store)
                   .HasForeignKey(d => d.StoreId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(s => s.Categories)
                   .WithOne(c => c.Store)
                   .HasForeignKey(c => c.StoreId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(s => s.Products)
                   .WithOne(p => p.Store)
                   .HasForeignKey(p => p.StoreId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.HasKey(c => c.Id);
            builder.Property(c => c.Name).IsRequired().HasMaxLength(100);
            builder.Property(c => c.Description).HasMaxLength(500);
            builder.Property(c => c.ColorHex).HasMaxLength(10);

            // Self-referencing relationship for Main Category -> Sub Categories
            builder.HasOne(c => c.ParentCategory)
                   .WithMany(p => p.SubCategories)
                   .HasForeignKey(c => c.ParentCategoryId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Category has many products. Deletion behavior is managed via business rule.
            builder.HasMany(c => c.Products)
                   .WithOne(p => p.Category)
                   .HasForeignKey(p => p.CategoryId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Ensure category name is unique per store per parent level
            builder.HasIndex(c => new { c.StoreId, c.ParentCategoryId, c.Name }).IsUnique();
        }
    }

    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Name).IsRequired().HasMaxLength(150);
            builder.Property(p => p.Barcode).IsRequired().HasMaxLength(100);
            builder.Property(p => p.Price).HasColumnType("decimal(18,2)");
            builder.Property(p => p.Stock).HasDefaultValue(0);

            // Barcode must be unique per store
            builder.HasIndex(p => new { p.StoreId, p.Barcode }).IsUnique();
        }
    }

    public class DeviceConfiguration : IEntityTypeConfiguration<Device>
    {
        public void Configure(EntityTypeBuilder<Device> builder)
        {
            builder.HasKey(d => d.Id);
            builder.Property(d => d.DeviceCode).IsRequired().HasMaxLength(50);
            builder.Property(d => d.Name).IsRequired().HasMaxLength(100);
        }
    }
}

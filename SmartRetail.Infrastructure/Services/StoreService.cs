using Microsoft.EntityFrameworkCore;
using SmartRetail.Application.DTOs.Store;
using SmartRetail.Application.Interfaces.Services;
using SmartRetail.Domain.Entities;
using SmartRetail.Infrastructure.Persistence.Contexts;

namespace SmartRetail.Infrastructure.Services
{
    public class StoreService : IStoreService
    {
        private readonly AppDbContext _context;
        private readonly ICategoryService _categoryService;

        public StoreService(AppDbContext context, ICategoryService categoryService)
        {
            _context = context;
            _categoryService = categoryService;
        }

        public async Task<IEnumerable<StoreDto>> GetOwnerStoresAsync(string ownerId)
        {
            return await _context.Stores
                .Where(s => s.OwnerId == ownerId)
                .Select(s => new StoreDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Address = s.Address,
                    City = s.City,
                    Latitude = s.Latitude,
                    Longitude = s.Longitude,
                    IsActive = s.IsActive,
                    CreatedAt = s.CreatedAt,
                    CategoriesCount = s.Categories.Count,
                    ProductsCount = s.Products.Count,
                    DevicesCount = s.Devices.Count
                })
                .ToListAsync();
        }

        public async Task<StoreDto?> GetStoreByIdAsync(int id, string ownerId)
        {
            var store = await _context.Stores
                .Include(s => s.Categories)
                .Include(s => s.Products)
                .Include(s => s.Devices)
                .FirstOrDefaultAsync(s => s.Id == id && s.OwnerId == ownerId);

            if (store == null) return null;

            return new StoreDto
            {
                Id = store.Id,
                Name = store.Name,
                Address = store.Address,
                City = store.City,
                Latitude = store.Latitude,
                Longitude = store.Longitude,
                IsActive = store.IsActive,
                CreatedAt = store.CreatedAt,
                CategoriesCount = store.Categories.Count,
                ProductsCount = store.Products.Count,
                DevicesCount = store.Devices.Count
            };
        }

        public async Task<StoreDto> CreateStoreAsync(string ownerId, CreateStoreDto dto)
        {
            var store = new Store
            {
                Name = dto.Name.Trim(),
                Address = dto.Address,
                City = dto.City,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                IsActive = true,
                OwnerId = ownerId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Stores.Add(store);
            await _context.SaveChangesAsync();

            // Automatically ensure default "General" category exists for the newly created store
            await _categoryService.EnsureDefaultCategoryExistsAsync(store.Id);

            return new StoreDto
            {
                Id = store.Id,
                Name = store.Name,
                Address = store.Address,
                City = store.City,
                Latitude = store.Latitude,
                Longitude = store.Longitude,
                IsActive = store.IsActive,
                CreatedAt = store.CreatedAt,
                CategoriesCount = 1,
                ProductsCount = 0,
                DevicesCount = 0
            };
        }

        public async Task<StoreDto?> UpdateStoreAsync(int id, string ownerId, UpdateStoreDto dto)
        {
            var store = await _context.Stores.FirstOrDefaultAsync(s => s.Id == id && s.OwnerId == ownerId);
            if (store == null) return null;

            store.Name = dto.Name.Trim();
            store.Address = dto.Address;
            store.City = dto.City;
            store.Latitude = dto.Latitude;
            store.Longitude = dto.Longitude;
            store.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();

            return await GetStoreByIdAsync(id, ownerId);
        }

        public async Task<bool> DeleteStoreAsync(int id, string ownerId)
        {
            var store = await _context.Stores
                .Include(s => s.Categories)
                .Include(s => s.Products)
                .FirstOrDefaultAsync(s => s.Id == id && s.OwnerId == ownerId);

            if (store == null) return false;

            _context.Products.RemoveRange(store.Products);
            _context.Categories.RemoveRange(store.Categories);
            _context.Stores.Remove(store);

            await _context.SaveChangesAsync();
            return true;
        }
    }
}

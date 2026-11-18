using Microsoft.EntityFrameworkCore;
using SmartRetail.Application.DTOs.Category;
using SmartRetail.Application.Interfaces.Services;
using SmartRetail.Domain.Entities;
using SmartRetail.Infrastructure.Persistence.Contexts;

namespace SmartRetail.Infrastructure.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly AppDbContext _context;

        public CategoryService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<CategoryDto> EnsureDefaultCategoryExistsAsync(int storeId)
        {
            var defaultCategory = await _context.Categories
                .FirstOrDefaultAsync(c => c.StoreId == storeId && c.IsDefault);

            if (defaultCategory == null)
            {
                defaultCategory = new Category
                {
                    Name = "متنوعة",
                    Description = "الفئة الافتراضية العامة للمنتجات غير المصنفة",
                    StoreId = storeId,
                    ParentCategoryId = null,
                    IsDefault = true,
                    IsVisible = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Categories.Add(defaultCategory);
                await _context.SaveChangesAsync();
            }

            return MapToDto(defaultCategory);
        }

        public async Task<IEnumerable<CategoryDto>> GetStoreCategoriesAsync(int storeId, bool onlyVisible = false)
        {
            var query = _context.Categories
                .Include(c => c.SubCategories)
                .Include(c => c.Products)
                .Include(c => c.ParentCategory)
                .Where(c => c.StoreId == storeId);

            if (onlyVisible)
            {
                query = query.Where(c => c.IsVisible);
            }

            var allCategories = await query.ToListAsync();

            // Return top-level (ParentCategoryId == null) with nested SubCategories
            var topLevel = allCategories.Where(c => c.ParentCategoryId == null);

            return topLevel.Select(c => MapToDtoWithChildren(c, allCategories)).ToList();
        }

        public async Task<CategoryDto?> GetCategoryByIdAsync(int id)
        {
            var category = await _context.Categories
                .Include(c => c.SubCategories)
                .Include(c => c.Products)
                .Include(c => c.ParentCategory)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null) return null;

            return MapToDto(category);
        }

        public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto dto)
        {
            // Ensure default exists for store just in case
            await EnsureDefaultCategoryExistsAsync(dto.StoreId);

            // Check if name already exists under same parent
            var exists = await _context.Categories.AnyAsync(c =>
                c.StoreId == dto.StoreId &&
                c.ParentCategoryId == dto.ParentCategoryId &&
                c.Name.Trim().ToLower() == dto.Name.Trim().ToLower());

            if (exists)
            {
                throw new InvalidOperationException($"توجد فئة بهذا الاسم '{dto.Name}' بالفعل في نفس المستوى.");
            }

            var category = new Category
            {
                Name = dto.Name.Trim(),
                Description = dto.Description,
                StoreId = dto.StoreId,
                ParentCategoryId = dto.ParentCategoryId,
                GridX = dto.GridX,
                GridY = dto.GridY,
                Width = dto.Width,
                Height = dto.Height,
                ColorHex = dto.ColorHex,
                IsDefault = false,
                IsVisible = dto.IsVisible,
                CreatedAt = DateTime.UtcNow
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return MapToDto(category);
        }

        public async Task<CategoryDto?> UpdateCategoryAsync(int id, UpdateCategoryDto dto)
        {
            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
            if (category == null) return null;

            if (category.IsDefault && category.Name != dto.Name)
            {
                // Can't rename the default "General" category to something invalid or delete it
            }

            category.Name = dto.Name.Trim();
            category.Description = dto.Description;
            category.ParentCategoryId = dto.ParentCategoryId;
            category.GridX = dto.GridX;
            category.GridY = dto.GridY;
            category.Width = dto.Width;
            category.Height = dto.Height;
            category.ColorHex = dto.ColorHex;
            category.IsVisible = dto.IsVisible;

            await _context.SaveChangesAsync();
            return MapToDto(category);
        }

        public async Task<CategoryDto?> UpdateCategoryPositionAsync(int id, UpdateCategoryPositionDto dto)
        {
            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
            if (category == null) return null;

            category.GridX = dto.GridX;
            category.GridY = dto.GridY;
            category.Width = dto.Width;
            category.Height = dto.Height;
            if (!string.IsNullOrWhiteSpace(dto.ColorHex))
            {
                category.ColorHex = dto.ColorHex;
            }

            await _context.SaveChangesAsync();
            return MapToDto(category);
        }

        public async Task<bool> ToggleVisibilityAsync(int id, bool isVisible)
        {
            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
            if (category == null) return false;

            category.IsVisible = isVisible;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<(bool Success, string Message)> DeleteCategoryAsync(int id, bool deleteProducts)
        {
            var category = await _context.Categories
                .Include(c => c.Products)
                .Include(c => c.SubCategories)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                return (false, "الفئة غير موجودة.");
            }

            if (category.IsDefault)
            {
                return (false, "لا يمكن حذف الفئة الافتراضية (متنوعة). يمكنك إخفاؤها فقط.");
            }

            // Find the store default category
            var defaultCategory = await _context.Categories
                .FirstOrDefaultAsync(c => c.StoreId == category.StoreId && c.IsDefault);

            if (defaultCategory == null)
            {
                // Auto create default if missing
                var defaultDto = await EnsureDefaultCategoryExistsAsync(category.StoreId);
                defaultCategory = await _context.Categories.FindAsync(defaultDto.Id);
            }

            // Also handle any subcategories: reparent them or cascade
            foreach (var sub in category.SubCategories.ToList())
            {
                sub.ParentCategoryId = category.ParentCategoryId;
            }

            if (deleteProducts)
            {
                // Delete all products associated with this category
                _context.Products.RemoveRange(category.Products);
            }
            else
            {
                // Transfer products to default "General" category
                foreach (var product in category.Products)
                {
                    product.CategoryId = defaultCategory!.Id;
                }
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return (true, deleteProducts
                ? "تم حذف الفئة وحذف جميع المنتجات التابعة لها بنجاح."
                : "تم حذف الفئة ونقل جميع منتجاتها إلى فئة 'متنوعة' بنجاح.");
        }

        public async Task<bool> SaveStoreMapLayoutAsync(int storeId, StoreMapLayoutDto dto)
        {
            var store = await _context.Stores
                .Include(s => s.Categories)
                .FirstOrDefaultAsync(s => s.Id == storeId);

            if (store == null) return false;

            if (dto.EntranceX.HasValue && dto.EntranceY.HasValue)
            {
                store.Latitude = dto.EntranceX.Value.ToString();
                store.Longitude = dto.EntranceY.Value.ToString();
            }

            if (dto.Positions != null && dto.Positions.Any())
            {
                var categoryMap = store.Categories.ToDictionary(c => c.Id);
                foreach (var pos in dto.Positions)
                {
                    if (categoryMap.TryGetValue(pos.CategoryId, out var cat))
                    {
                        cat.GridX = pos.GridX;
                        cat.GridY = pos.GridY;
                    }
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }

        private static CategoryDto MapToDto(Category c)
        {
            return new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                StoreId = c.StoreId,
                ParentCategoryId = c.ParentCategoryId,
                ParentCategoryName = c.ParentCategory?.Name,
                GridX = c.GridX,
                GridY = c.GridY,
                Width = c.Width,
                Height = c.Height,
                ColorHex = c.ColorHex,
                IsDefault = c.IsDefault,
                IsVisible = c.IsVisible,
                CreatedAt = c.CreatedAt,
                ProductsCount = c.Products?.Count ?? 0,
                SubCategories = c.SubCategories?.Select(MapToDto).ToList() ?? new()
            };
        }

        private static CategoryDto MapToDtoWithChildren(Category c, List<Category> allCategories)
        {
            var dto = MapToDto(c);
            var children = allCategories.Where(sub => sub.ParentCategoryId == c.Id).ToList();
            dto.SubCategories = children.Select(sub => MapToDtoWithChildren(sub, allCategories)).ToList();
            return dto;
        }
    }
}

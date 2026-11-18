using Microsoft.EntityFrameworkCore;
using SmartRetail.Application.DTOs.Product;
using SmartRetail.Application.Interfaces.Services;
using SmartRetail.Domain.Entities;
using SmartRetail.Infrastructure.Persistence.Contexts;

namespace SmartRetail.Infrastructure.Services
{
    public class ProductService : IProductService
    {
        private readonly AppDbContext _context;
        private readonly ICategoryService _categoryService;

        public ProductService(AppDbContext context, ICategoryService categoryService)
        {
            _context = context;
            _categoryService = categoryService;
        }

        public async Task<IEnumerable<ProductDto>> GetStoreProductsAsync(int storeId, int? categoryId = null, bool onlyVisible = false)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .Where(p => p.StoreId == storeId);

            if (categoryId.HasValue)
            {
                var subCategoryIds = await _context.Categories
                    .Where(c => c.ParentCategoryId == categoryId.Value)
                    .Select(c => c.Id)
                    .ToListAsync();

                if (subCategoryIds.Any())
                {
                    query = query.Where(p => p.CategoryId == categoryId.Value || subCategoryIds.Contains(p.CategoryId));
                }
                else
                {
                    query = query.Where(p => p.CategoryId == categoryId.Value);
                }
            }

            if (onlyVisible)
            {
                query = query.Where(p => p.IsVisible);
            }

            return await query.Select(p => MapToDto(p)).ToListAsync();
        }

        public async Task<ProductDto?> GetProductByIdAsync(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            return product == null ? null : MapToDto(product);
        }

        public async Task<ProductDto?> GetProductByBarcodeAsync(int storeId, string barcode)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.StoreId == storeId && p.Barcode == barcode.Trim());

            return product == null ? null : MapToDto(product);
        }

        public async Task<ProductDto> CreateProductAsync(CreateProductDto dto)
        {
            // Barcode uniqueness per store check
            var barcodeExists = await _context.Products.AnyAsync(p =>
                p.StoreId == dto.StoreId && p.Barcode == dto.Barcode.Trim());

            if (barcodeExists)
            {
                throw new InvalidOperationException($"الباركود '{dto.Barcode}' مسجل مسبقاً في هذا المتجر.");
            }

            // Ensure category exists or default to "General"
            int finalCategoryId;
            if (dto.CategoryId.HasValue && dto.CategoryId.Value > 0)
            {
                var catExists = await _context.Categories.AnyAsync(c => c.Id == dto.CategoryId.Value && c.StoreId == dto.StoreId);
                if (!catExists)
                {
                    throw new InvalidOperationException("الفئة المحددة غير موجودة في هذا المتجر.");
                }
                finalCategoryId = dto.CategoryId.Value;
            }
            else
            {
                var defaultCat = await _categoryService.EnsureDefaultCategoryExistsAsync(dto.StoreId);
                finalCategoryId = defaultCat.Id;
            }

            var product = new Product
            {
                Name = dto.Name.Trim(),
                Description = dto.Description,
                Barcode = dto.Barcode.Trim(),
                Price = dto.Price,
                Stock = dto.Stock,
                IsVisible = dto.IsVisible,
                StoreId = dto.StoreId,
                CategoryId = finalCategoryId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // Load category for DTO
            await _context.Entry(product).Reference(p => p.Category).LoadAsync();

            return MapToDto(product);
        }

        public async Task<ProductDto?> UpdateProductAsync(int id, UpdateProductDto dto)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) return null;

            // Barcode uniqueness per store check (if barcode changed)
            if (product.Barcode != dto.Barcode.Trim())
            {
                var barcodeExists = await _context.Products.AnyAsync(p =>
                    p.StoreId == product.StoreId && p.Id != id && p.Barcode == dto.Barcode.Trim());

                if (barcodeExists)
                {
                    throw new InvalidOperationException($"الباركود '{dto.Barcode}' مسجل مسبقاً في هذا المتجر.");
                }
            }

            // Ensure target category exists in the same store
            var catExists = await _context.Categories.AnyAsync(c => c.Id == dto.CategoryId && c.StoreId == product.StoreId);
            if (!catExists)
            {
                throw new InvalidOperationException("الفئة المحددة غير موجودة في هذا المتجر.");
            }

            product.Name = dto.Name.Trim();
            product.Description = dto.Description;
            product.Barcode = dto.Barcode.Trim();
            product.Price = dto.Price;
            product.Stock = dto.Stock;
            product.IsVisible = dto.IsVisible;
            product.CategoryId = dto.CategoryId;

            await _context.SaveChangesAsync();
            await _context.Entry(product).Reference(p => p.Category).LoadAsync();

            return MapToDto(product);
        }

        public async Task<bool> ToggleVisibilityAsync(int id, bool isVisible)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) return false;

            product.IsVisible = isVisible;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) return false;

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return true;
        }

        private static ProductDto MapToDto(Product p)
        {
            return new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Barcode = p.Barcode,
                Price = p.Price,
                Stock = p.Stock,
                IsVisible = p.IsVisible,
                StoreId = p.StoreId,
                CategoryId = p.CategoryId,
                CategoryName = p.Category?.Name ?? string.Empty,
                CreatedAt = p.CreatedAt
            };
        }
    }
}

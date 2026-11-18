using SmartRetail.Application.DTOs.Product;

namespace SmartRetail.Application.Interfaces.Services
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDto>> GetStoreProductsAsync(int storeId, int? categoryId = null, bool onlyVisible = false);
        Task<ProductDto?> GetProductByIdAsync(int id);
        Task<ProductDto?> GetProductByBarcodeAsync(int storeId, string barcode);
        Task<ProductDto> CreateProductAsync(CreateProductDto dto);
        Task<ProductDto?> UpdateProductAsync(int id, UpdateProductDto dto);
        Task<bool> ToggleVisibilityAsync(int id, bool isVisible);
        Task<bool> DeleteProductAsync(int id);
    }
}

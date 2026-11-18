using SmartRetail.Application.DTOs.Category;

namespace SmartRetail.Application.Interfaces.Services
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryDto>> GetStoreCategoriesAsync(int storeId, bool onlyVisible = false);
        Task<CategoryDto?> GetCategoryByIdAsync(int id);
        Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto dto);
        Task<CategoryDto?> UpdateCategoryAsync(int id, UpdateCategoryDto dto);
        Task<CategoryDto?> UpdateCategoryPositionAsync(int id, UpdateCategoryPositionDto dto);
        Task<bool> ToggleVisibilityAsync(int id, bool isVisible);
        Task<(bool Success, string Message)> DeleteCategoryAsync(int id, bool deleteProducts);
        Task<CategoryDto> EnsureDefaultCategoryExistsAsync(int storeId);
        Task<bool> SaveStoreMapLayoutAsync(int storeId, StoreMapLayoutDto dto);
    }
}

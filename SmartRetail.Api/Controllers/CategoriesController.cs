using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartRetail.Api.Helpers;
using SmartRetail.Application.DTOs.Category;
using SmartRetail.Application.Interfaces.Services;

namespace SmartRetail.Api.Controllers
{
    [Route("api/categories")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        /// <summary>
        /// Get all categories for a store with hierarchy and layout coordinates
        /// </summary>
        [HttpGet("store/{storeId:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetStoreCategories(int storeId, [FromQuery] bool onlyVisible = false)
        {
            var categories = await _categoryService.GetStoreCategoriesAsync(storeId, onlyVisible);
            return Ok(new APIResponse<IEnumerable<CategoryDto>>(200, "Categories retrieved successfully.", categories));
        }

        /// <summary>
        /// Get category by ID
        /// </summary>
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCategoryById(int id)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);
            if (category == null)
            {
                return NotFound(new APIResponse<object>(404, "الفئة غير موجودة."));
            }

            return Ok(new APIResponse<CategoryDto>(200, "Category retrieved successfully.", category));
        }

        /// <summary>
        /// Create a new category or subcategory with layout coordinates
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Owner,SuperAdmin")]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                return BadRequest(new APIResponse<object>(400, "اسم الفئة مطلوب."));
            }

            try
            {
                var created = await _categoryService.CreateCategoryAsync(dto);
                return CreatedAtAction(nameof(GetCategoryById), new { id = created.Id },
                    new APIResponse<CategoryDto>(201, "تم إنشاء الفئة بنجاح.", created));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new APIResponse<object>(400, ex.Message));
            }
        }

        /// <summary>
        /// Update category details
        /// </summary>
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Owner,SuperAdmin")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateCategoryDto dto)
        {
            var updated = await _categoryService.UpdateCategoryAsync(id, dto);
            if (updated == null)
            {
                return NotFound(new APIResponse<object>(404, "الفئة غير موجودة."));
            }

            return Ok(new APIResponse<CategoryDto>(200, "تم تعديل الفئة بنجاح.", updated));
        }

        /// <summary>
        /// Update category layout position on store map
        /// </summary>
        [HttpPatch("{id:int}/position")]
        [Authorize(Roles = "Owner,SuperAdmin")]
        public async Task<IActionResult> UpdatePosition(int id, [FromBody] UpdateCategoryPositionDto dto)
        {
            var updated = await _categoryService.UpdateCategoryPositionAsync(id, dto);
            if (updated == null)
            {
                return NotFound(new APIResponse<object>(404, "الفئة غير موجودة."));
            }

            return Ok(new APIResponse<CategoryDto>(200, "تم تحديث موضع الفئة في الخريطة بنجاح.", updated));
        }

        /// <summary>
        /// Toggle category visibility
        /// </summary>
        [HttpPatch("{id:int}/visibility")]
        [Authorize(Roles = "Owner,SuperAdmin")]
        public async Task<IActionResult> ToggleVisibility(int id, [FromBody] ToggleVisibilityDto dto)
        {
            var success = await _categoryService.ToggleVisibilityAsync(id, dto.IsVisible);
            if (!success)
            {
                return NotFound(new APIResponse<object>(404, "الفئة غير موجودة."));
            }

            return Ok(new APIResponse<object>(200, dto.IsVisible ? "تم إظهار الفئة بنجاح." : "تم إخفاء الفئة بنجاح."));
        }

        /// <summary>
        /// Delete category with option to reassign products to default
        /// Default category cannot be deleted
        /// </summary>
        /// <param name="id">Category ID to delete</param>
        /// <param name="deleteProducts">Whether to delete products or reassign</param>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Owner,SuperAdmin")]
        public async Task<IActionResult> DeleteCategory(int id, [FromQuery] bool deleteProducts = false)
        {
            var result = await _categoryService.DeleteCategoryAsync(id, deleteProducts);
            if (!result.Success)
            {
                return BadRequest(new APIResponse<object>(400, result.Message));
            }

            return Ok(new APIResponse<object>(200, result.Message));
        }

        /// <summary>
        /// Ensure default category exists for store
        /// </summary>
        [HttpPost("store/{storeId:int}/ensure-default")]
        [Authorize(Roles = "Owner,SuperAdmin")]
        public async Task<IActionResult> EnsureDefaultCategory(int storeId)
        {
            var defaultCat = await _categoryService.EnsureDefaultCategoryExistsAsync(storeId);
            return Ok(new APIResponse<CategoryDto>(200, "تم التحقق من وجود الفئة الافتراضية بنجاح.", defaultCat));
        }

        /// <summary>
        /// Save batch store map layout and entrance position to database
        /// </summary>
        [HttpPost("store/{storeId:int}/layout")]
        [Authorize(Roles = "Owner,SuperAdmin")]
        public async Task<IActionResult> SaveStoreMapLayout(int storeId, [FromBody] StoreMapLayoutDto dto)
        {
            var success = await _categoryService.SaveStoreMapLayoutAsync(storeId, dto);
            if (!success)
            {
                return NotFound(new APIResponse<object>(404, "المتجر غير موجود."));
            }

            return Ok(new APIResponse<object>(200, "تم حفظ تخطيط ومواقع الخريطة بنجاح في قاعدة البيانات."));
        }
    }
}

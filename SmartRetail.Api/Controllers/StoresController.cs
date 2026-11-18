using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartRetail.Api.Helpers;
using SmartRetail.Application.DTOs.Store;
using SmartRetail.Application.Interfaces.Services;

namespace SmartRetail.Api.Controllers
{
    [Route("api/stores")]
    [ApiController]
    [Authorize(Roles = "Owner,SuperAdmin")]
    public class StoresController : ControllerBase
    {
        private readonly IStoreService _storeService;

        public StoresController(IStoreService storeService)
        {
            _storeService = storeService;
        }

        private string? GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        /// <summary>
        /// Get all stores for current owner
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetMyStores()
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new APIResponse<object>(401, "المستخدم غير مصرح له."));
            }

            var stores = await _storeService.GetOwnerStoresAsync(userId);
            return Ok(new APIResponse<IEnumerable<StoreDto>>(200, "Stores retrieved successfully.", stores));
        }

        /// <summary>
        /// Get store by ID
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetStoreById(int id)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new APIResponse<object>(401, "المستخدم غير مصرح له."));
            }

            var store = await _storeService.GetStoreByIdAsync(id, userId);
            if (store == null)
            {
                return NotFound(new APIResponse<object>(404, "المتجر غير موجود أو لا تملك صلاحية الوصول إليه."));
            }

            return Ok(new APIResponse<StoreDto>(200, "Store retrieved successfully.", store));
        }

        /// <summary>
        /// Create new store for owner
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateStore([FromBody] CreateStoreDto dto)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new APIResponse<object>(401, "المستخدم غير مصرح له."));
            }

            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                return BadRequest(new APIResponse<object>(400, "اسم المتجر مطلوب."));
            }

            var store = await _storeService.CreateStoreAsync(userId, dto);
            return CreatedAtAction(nameof(GetStoreById), new { id = store.Id },
                new APIResponse<StoreDto>(201, "تم إنشاء المتجر بنجاح.", store));
        }

        /// <summary>
        /// Update store info
        /// </summary>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateStore(int id, [FromBody] UpdateStoreDto dto)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new APIResponse<object>(401, "المستخدم غير مصرح له."));
            }

            var updated = await _storeService.UpdateStoreAsync(id, userId, dto);
            if (updated == null)
            {
                return NotFound(new APIResponse<object>(404, "المتجر غير موجود أو لا تملك صلاحية تعديله."));
            }

            return Ok(new APIResponse<StoreDto>(200, "تم تعديل بيانات المتجر بنجاح.", updated));
        }

        /// <summary>
        /// Delete store
        /// </summary>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteStore(int id)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new APIResponse<object>(401, "المستخدم غير مصرح له."));
            }

            var success = await _storeService.DeleteStoreAsync(id, userId);
            if (!success)
            {
                return NotFound(new APIResponse<object>(404, "المتجر غير موجود."));
            }

            return Ok(new APIResponse<object>(200, "تم حذف المتجر وجميع بياناته بنجاح."));
        }
    }
}

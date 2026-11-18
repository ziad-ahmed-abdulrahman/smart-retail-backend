using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartRetail.Api.Helpers;
using SmartRetail.Application.DTOs.Device;
using SmartRetail.Application.Interfaces.Services;

namespace SmartRetail.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DevicesController : ControllerBase
    {
        private readonly IDeviceService _deviceService;

        public DevicesController(IDeviceService deviceService)
        {
            _deviceService = deviceService;
        }

        private string GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        }

        /// <summary>
        /// Get all registered devices for a store
        /// </summary>
        [HttpGet("store/{storeId:int}")]
        [Authorize(Roles = "Owner,SuperAdmin")]
        public async Task<IActionResult> GetStoreDevices(int storeId)
        {
            var userId = GetCurrentUserId();
            var devices = await _deviceService.GetStoreDevicesAsync(storeId, userId);
            return Ok(new APIResponse<IEnumerable<DeviceDto>>(200, "تم جلب أجهزة الفرع بنجاح.", devices));
        }

        /// <summary>
        /// Get device by ID
        /// </summary>
        [HttpGet("{id:int}")]
        [Authorize(Roles = "Owner,SuperAdmin")]
        public async Task<IActionResult> GetDevice(int id)
        {
            var userId = GetCurrentUserId();
            var device = await _deviceService.GetDeviceByIdAsync(id, userId);
            if (device == null)
            {
                return NotFound(new APIResponse<object>(404, "الجهاز غير موجود."));
            }

            return Ok(new APIResponse<DeviceDto>(200, "تم جلب بيانات الجهاز بنجاح.", device));
        }

        /// <summary>
        /// Register new device for store
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Owner,SuperAdmin")]
        public async Task<IActionResult> CreateDevice([FromBody] CreateDeviceDto dto)
        {
            var userId = GetCurrentUserId();
            try
            {
                var created = await _deviceService.CreateDeviceAsync(userId, dto);
                return CreatedAtAction(nameof(GetDevice), new { id = created.Id }, new APIResponse<DeviceDto>(201, "تم تسجيل الجهاز بنجاح.", created));
            }
            catch (System.InvalidOperationException ex)
            {
                return BadRequest(new APIResponse<object>(400, ex.Message));
            }
        }

        /// <summary>
        /// Update device info
        /// </summary>
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Owner,SuperAdmin")]
        public async Task<IActionResult> UpdateDevice(int id, [FromBody] UpdateDeviceDto dto)
        {
            var userId = GetCurrentUserId();
            var updated = await _deviceService.UpdateDeviceAsync(id, userId, dto);
            if (updated == null)
            {
                return NotFound(new APIResponse<object>(404, "الجهاز غير موجود أو لا تملك صلاحية تعديله."));
            }

            return Ok(new APIResponse<DeviceDto>(200, "تم تحديث بيانات الجهاز بنجاح.", updated));
        }

        /// <summary>
        /// Delete device from store
        /// </summary>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Owner,SuperAdmin")]
        public async Task<IActionResult> DeleteDevice(int id)
        {
            var userId = GetCurrentUserId();
            var success = await _deviceService.DeleteDeviceAsync(id, userId);
            if (!success)
            {
                return NotFound(new APIResponse<object>(404, "الجهاز غير موجود أو لا تملك صلاحية حذفه."));
            }

            return Ok(new APIResponse<object>(200, "تم حذف الجهاز من الفرع بنجاح."));
        }

        /// <summary>
        /// Update device online status
        /// </summary>
        [HttpPatch("{id:int}/status")]
        [AllowAnonymous]
        public async Task<IActionResult> UpdateDeviceStatus(int id, [FromBody] UpdateDeviceStatusDto dto)
        {
            var success = await _deviceService.UpdateStatusAsync(id, dto.IsOnline);
            if (!success)
            {
                return NotFound(new APIResponse<object>(404, "الجهاز غير موجود."));
            }

            return Ok(new APIResponse<object>(200, "تم تحديث حالة اتصال الجهاز بنجاح."));
        }
    }
}

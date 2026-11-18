using System.Collections.Generic;
using System.Threading.Tasks;
using SmartRetail.Application.DTOs.Device;

namespace SmartRetail.Application.Interfaces.Services
{
    public interface IDeviceService
    {
        Task<IEnumerable<DeviceDto>> GetStoreDevicesAsync(int storeId, string ownerId);
        Task<DeviceDto?> GetDeviceByIdAsync(int id, string ownerId);
        Task<DeviceDto> CreateDeviceAsync(string ownerId, CreateDeviceDto dto);
        Task<DeviceDto?> UpdateDeviceAsync(int id, string ownerId, UpdateDeviceDto dto);
        Task<bool> DeleteDeviceAsync(int id, string ownerId);
        Task<bool> UpdateStatusAsync(int id, bool isOnline);
    }
}

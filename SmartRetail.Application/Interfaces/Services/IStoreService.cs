using SmartRetail.Application.DTOs.Store;

namespace SmartRetail.Application.Interfaces.Services
{
    public interface IStoreService
    {
        Task<IEnumerable<StoreDto>> GetOwnerStoresAsync(string ownerId);
        Task<StoreDto?> GetStoreByIdAsync(int id, string ownerId);
        Task<StoreDto> CreateStoreAsync(string ownerId, CreateStoreDto dto);
        Task<StoreDto?> UpdateStoreAsync(int id, string ownerId, UpdateStoreDto dto);
        Task<bool> DeleteStoreAsync(int id, string ownerId);
    }
}

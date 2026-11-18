namespace SmartRetail.Application.DTOs.Store
{
    public class StoreDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Latitude { get; set; }
        public string? Longitude { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public int CategoriesCount { get; set; }
        public int ProductsCount { get; set; }
        public int DevicesCount { get; set; }
    }

    public class CreateStoreDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Latitude { get; set; }
        public string? Longitude { get; set; }
    }

    public class UpdateStoreDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Latitude { get; set; }
        public string? Longitude { get; set; }
        public bool IsActive { get; set; }
    }
}

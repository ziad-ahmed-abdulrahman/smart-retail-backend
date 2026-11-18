namespace SmartRetail.Domain.Entities
{
    public class Store
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Latitude { get; set; }
        public string? Longitude { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Owner reference
        public string OwnerId { get; set; } = null!;
        public User Owner { get; set; } = null!;

        // Store Devices
        public ICollection<Device> Devices { get; set; } = new List<Device>();

        // Store Categories (with indoor map & layout)
        public ICollection<Category> Categories { get; set; } = new List<Category>();

        // Store Products
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}

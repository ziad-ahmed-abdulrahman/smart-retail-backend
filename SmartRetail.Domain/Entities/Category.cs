namespace SmartRetail.Domain.Entities
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        // Store reference
        public int StoreId { get; set; }
        public Store Store { get; set; } = null!;

        // Parent / Sub-category relationship (e.g. Meat -> Poultry / Fish)
        public int? ParentCategoryId { get; set; }
        public Category? ParentCategory { get; set; }
        public ICollection<Category> SubCategories { get; set; } = new List<Category>();

        // Store layout & indoor map coordinates
        public int? GridX { get; set; }
        public int? GridY { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public string? ColorHex { get; set; } // e.g. #b91c1c

        // Business rules:
        // IsDefault = true means "General / General" - cannot be deleted, but can be hidden
        public bool IsDefault { get; set; } = false;
        public bool IsVisible { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}

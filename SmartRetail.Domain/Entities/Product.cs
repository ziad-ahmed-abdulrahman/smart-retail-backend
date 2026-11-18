namespace SmartRetail.Domain.Entities
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Barcode { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Stock { get; set; }

        // Visibility rule (Owner can hide product anytime)
        public bool IsVisible { get; set; } = true;

        // Store reference
        public int StoreId { get; set; }
        public Store Store { get; set; } = null!;

        // Category reference (Mandatory: Product cannot exist without Category)
        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

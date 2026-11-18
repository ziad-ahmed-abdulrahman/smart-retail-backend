namespace SmartRetail.Application.DTOs.Product
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Barcode { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public bool IsVisible { get; set; }
        public int StoreId { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class CreateProductDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Barcode { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Stock { get; set; } = 0;
        public bool IsVisible { get; set; } = true;
        public int StoreId { get; set; }
        public int? CategoryId { get; set; } // If null, assigns to store's default "General" category
    }

    public class UpdateProductDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Barcode { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public bool IsVisible { get; set; }
        public int CategoryId { get; set; }
    }
}

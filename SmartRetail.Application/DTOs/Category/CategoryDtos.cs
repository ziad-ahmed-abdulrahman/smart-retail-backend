namespace SmartRetail.Application.DTOs.Category
{
    public class CategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int StoreId { get; set; }
        public int? ParentCategoryId { get; set; }
        public string? ParentCategoryName { get; set; }

        // Indoor map positioning & layout
        public int? GridX { get; set; }
        public int? GridY { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public string? ColorHex { get; set; }

        public bool IsDefault { get; set; }
        public bool IsVisible { get; set; }
        public DateTime CreatedAt { get; set; }

        public int ProductsCount { get; set; }
        public List<CategoryDto> SubCategories { get; set; } = new();
    }

    public class CreateCategoryDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int StoreId { get; set; }
        public int? ParentCategoryId { get; set; }

        // Layout / Coordinates
        public int? GridX { get; set; }
        public int? GridY { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public string? ColorHex { get; set; }

        public bool IsVisible { get; set; } = true;
    }

    public class UpdateCategoryDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? ParentCategoryId { get; set; }

        // Layout / Coordinates
        public int? GridX { get; set; }
        public int? GridY { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public string? ColorHex { get; set; }

        public bool IsVisible { get; set; }
    }

    public class UpdateCategoryPositionDto
    {
        public int? GridX { get; set; }
        public int? GridY { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public string? ColorHex { get; set; }
    }

    public class ToggleVisibilityDto
    {
        public bool IsVisible { get; set; }
    }

    public class StoreMapLayoutDto
    {
        public List<CategoryPositionItemDto> Positions { get; set; } = new();
        public int? EntranceX { get; set; }
        public int? EntranceY { get; set; }
    }

    public class CategoryPositionItemDto
    {
        public int CategoryId { get; set; }
        public int GridX { get; set; }
        public int GridY { get; set; }
    }
}

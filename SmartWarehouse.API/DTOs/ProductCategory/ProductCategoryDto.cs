namespace SmartWarehouse.API.DTOs.ProductCategory
{
    public class ProductCategoryDto
    {
        public Guid Id { get; set; }
        public string CompanyId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string ColorCode { get; set; } = "#607D8B";
        public string? IconName { get; set; }
        public int ProductCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

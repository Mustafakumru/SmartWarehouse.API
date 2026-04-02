namespace SmartWarehouse.API.DTOs.Product
{
    public class ProductFilterDto
    {
        public string CompanyId { get; set; } = string.Empty;
        public string? SearchTerm { get; set; }
        public Guid? CategoryId { get; set; }
        public bool? IsActive { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}

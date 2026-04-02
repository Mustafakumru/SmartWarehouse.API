namespace SmartWarehouse.API.DTOs.Product
{
    public class UpdateProductDto
    {
        public Guid Id { get; set; }
        public string CompanyId { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Unit { get; set; } = "Adet";
        public string? Barcode { get; set; }
        public decimal UnitCost { get; set; }
        public decimal UnitPrice { get; set; }
        public int MinStockLevel { get; set; }
        public int MaxStockLevel { get; set; }
        public bool IsActive { get; set; }
        public Guid ProductCategoryId { get; set; }
    }
}

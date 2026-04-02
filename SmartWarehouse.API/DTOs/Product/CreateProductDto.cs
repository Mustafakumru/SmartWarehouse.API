namespace SmartWarehouse.API.DTOs.Product
{
    public class CreateProductDto
    {
        public string CompanyId { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Unit { get; set; } = "Adet";
        public string? Barcode { get; set; }
        public decimal UnitCost { get; set; }
        public decimal UnitPrice { get; set; }
        public int MinStockLevel { get; set; } = 0;
        public int MaxStockLevel { get; set; } = 9999;
        public Guid ProductCategoryId { get; set; }
    }
}

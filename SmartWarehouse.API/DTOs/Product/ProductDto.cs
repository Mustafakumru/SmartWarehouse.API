namespace SmartWarehouse.API.DTOs.Product
{
    public class ProductDto
    {
        public Guid Id { get; set; }
        public string CompanyId { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Unit { get; set; } = string.Empty;
        public string? Barcode { get; set; }
        public decimal UnitCost { get; set; }
        public decimal UnitPrice { get; set; }
        public int MinStockLevel { get; set; }
        public int MaxStockLevel { get; set; }
        public bool IsActive { get; set; }
        public Guid ProductCategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string CategoryColorCode { get; set; } = string.Empty;

        // Anlık stok bilgisi (join ile gelir)
        public int TotalStock { get; set; }
        public bool IsLowStock { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}

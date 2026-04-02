namespace SmartWarehouse.API.DTOs.Product
{
    public class ProductListDto
    {
        public Guid Id { get; set; }
        public string SKU { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public bool IsActive { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string CategoryColorCode { get; set; } = string.Empty;
        public int TotalStock { get; set; }
        public int MinStockLevel { get; set; }
        public bool IsLowStock { get; set; }
    }
}

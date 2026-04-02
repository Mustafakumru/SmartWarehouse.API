namespace SmartWarehouse.API.DTOs.WarehouseStock
{
    public class WarehouseStockDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductSKU { get; set; } = string.Empty;
        public Guid WarehouseRackId { get; set; }
        public string RackCode { get; set; } = string.Empty;
        public string ZoneName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public int ReservedQuantity { get; set; }
        public int AvailableQuantity { get; set; }
        public DateTime LastMovementAt { get; set; }
    }
}

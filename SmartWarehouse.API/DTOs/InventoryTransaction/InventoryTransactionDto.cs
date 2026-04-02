using SmartWarehouse.API.Entities;

namespace SmartWarehouse.API.DTOs.InventoryTransaction
{
    public class InventoryTransactionDto
    {
        public Guid Id { get; set; }
        public string CompanyId { get; set; } = string.Empty;
        public string TransactionCode { get; set; } = string.Empty;
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductSKU { get; set; } = string.Empty;
        public Guid WarehouseRackId { get; set; }
        public string RackCode { get; set; } = string.Empty;
        public string ZoneName { get; set; } = string.Empty;
        public TransactionType TransactionType { get; set; }
        public string TransactionTypeName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitCost { get; set; }
        public decimal TotalCost { get; set; }
        public string? ReferenceNumber { get; set; }
        public string? Notes { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public int StockAfterTransaction { get; set; }
        public DateTime TransactionDate { get; set; }
    }
}

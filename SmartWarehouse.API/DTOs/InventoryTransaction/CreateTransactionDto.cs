using SmartWarehouse.API.Entities;

namespace SmartWarehouse.API.DTOs.InventoryTransaction
{
    public class CreateTransactionDto
    {
        public string CompanyId { get; set; } = string.Empty;
        public Guid ProductId { get; set; }
        public Guid WarehouseRackId { get; set; }
        public TransactionType TransactionType { get; set; }
        public int Quantity { get; set; }
        public decimal UnitCost { get; set; }
        public string? ReferenceNumber { get; set; }
        public string? Notes { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
    }
}

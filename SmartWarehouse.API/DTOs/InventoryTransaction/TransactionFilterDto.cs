using SmartWarehouse.API.Entities;

namespace SmartWarehouse.API.DTOs.InventoryTransaction
{
    public class TransactionFilterDto
    {
        public string CompanyId { get; set; } = string.Empty;
        public Guid? ProductId { get; set; }
        public TransactionType? TransactionType { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}

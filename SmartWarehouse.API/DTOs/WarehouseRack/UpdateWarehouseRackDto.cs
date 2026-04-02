namespace SmartWarehouse.API.DTOs.WarehouseRack
{
    public class UpdateWarehouseRackDto
    {
        public Guid Id { get; set; }
        public string CompanyId { get; set; } = string.Empty;
        public string RackCode { get; set; } = string.Empty;
        public string RackName { get; set; } = string.Empty;
        public int MaxCapacity { get; set; }
        public string RackType { get; set; } = "Standard";
        public bool IsActive { get; set; }
        public Guid WarehouseZoneId { get; set; }
    }
}

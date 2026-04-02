namespace SmartWarehouse.API.DTOs.WarehouseRack
{
    public class CreateWarehouseRackDto
    {
        public string CompanyId { get; set; } = string.Empty;
        public string RackCode { get; set; } = string.Empty;
        public string RackName { get; set; } = string.Empty;
        public int MaxCapacity { get; set; }
        public string RackType { get; set; } = "Standard";
        public Guid WarehouseZoneId { get; set; }
    }
}

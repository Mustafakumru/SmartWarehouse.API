namespace SmartWarehouse.API.DTOs.WarehouseZone
{

    public class UpdateWarehouseZoneDto
    {
        public Guid Id { get; set; }
        public string CompanyId { get; set; } = string.Empty;
        public string ZoneCode { get; set; } = string.Empty;
        public string ZoneName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int MaxCapacity { get; set; }
        public string? TemperatureRequirement { get; set; }
        public bool IsActive { get; set; }
    }
}

// Entities/WarehouseZone.cs
namespace SmartWarehouse.API.Entities;

/// <summary>
/// Depo Bölgesi. Örn: Zona-A (Soğuk Depo), Zona-B (Kuru Depo)
/// Fiziksel olarak birbirinden ayrılmış depo alanlarını temsil eder.
/// </summary>
public class WarehouseZone : BaseEntity
{
    /// <summary>
    /// Kısa bölge kodu. Örn: "ZA", "ZB", "COLD"
    /// </summary>
    public string ZoneCode { get; set; } = string.Empty;

    public string ZoneName { get; set; } = string.Empty;
    public string? Description { get; set; }

    /// <summary>
    /// Bölgenin toplam kapasitesi (birim cinsinden)
    /// </summary>
    public int MaxCapacity { get; set; }

    /// <summary>
    /// Sıcaklık gereksinimleri olan depolar için. Örn: "2-8°C", "Oda Sıcaklığı"
    /// </summary>
    public string? TemperatureRequirement { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation Property
    public ICollection<WarehouseRack> Racks { get; set; } = new List<WarehouseRack>();
}
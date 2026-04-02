// Entities/WarehouseRack.cs
namespace SmartWarehouse.API.Entities;

/// <summary>
/// Raf. Her bölgede (Zone) birden fazla raf bulunabilir.
/// Stok lokasyon bazında takip edilir: hangi ürün, hangi rafta.
/// </summary>
public class WarehouseRack : BaseEntity
{
    /// <summary>
    /// Raf kodu. Örn: "ZA-R01", "ZB-R05"
    /// </summary>
    public string RackCode { get; set; } = string.Empty;

    public string RackName { get; set; } = string.Empty;

    /// <summary>
    /// Rafın kaç ürün birimi alabileceği maksimum kapasite
    /// </summary>
    public int MaxCapacity { get; set; }

    /// <summary>
    /// Raf tipi: Standard, Heavy-Duty, Refrigerated
    /// </summary>
    public string RackType { get; set; } = "Standard";

    public bool IsActive { get; set; } = true;

    // Foreign Key
    public Guid WarehouseZoneId { get; set; }

    // Navigation Properties
    public WarehouseZone WarehouseZone { get; set; } = null!;
    public ICollection<WarehouseStock> Stocks { get; set; } = new List<WarehouseStock>();
    public ICollection<InventoryTransaction> Transactions { get; set; } = new List<InventoryTransaction>();
}
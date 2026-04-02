// Entities/WarehouseStock.cs
namespace SmartWarehouse.API.Entities;

/// <summary>
/// Anlık Stok Durumu. Hangi ürünün hangi rafta kaç adet olduğunu tutar.
/// Bu tablo InventoryTransaction'ların özeti niteliğindedir.
/// Product + WarehouseRack kombinasyonu CompanyId bazında unique olmalı.
/// </summary>
public class WarehouseStock : BaseEntity
{
    public Guid ProductId { get; set; }
    public Guid WarehouseRackId { get; set; }

    /// <summary>
    /// Anlık stok miktarı. Giriş işlemlerinde artar, çıkış işlemlerinde azalır.
    /// </summary>
    public int Quantity { get; set; } = 0;

    /// <summary>
    /// Son stok hareketinin tarihi. Cache geçersizleştirme için kullanılabilir.
    /// </summary>
    public DateTime LastMovementAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Bu stok kaydının rezerve edilmiş miktarı (sipariş için bekleyen)
    /// </summary>
    public int ReservedQuantity { get; set; } = 0;

    /// <summary>
    /// Kullanılabilir miktar = Quantity - ReservedQuantity
    /// Hesaplanan alan, veritabanına yazılmaz.
    /// </summary>
    public int AvailableQuantity => Quantity - ReservedQuantity;

    // Navigation Properties
    public Product Product { get; set; } = null!;
    public WarehouseRack WarehouseRack { get; set; } = null!;
}
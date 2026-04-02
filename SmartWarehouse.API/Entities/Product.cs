// Entities/Product.cs
namespace SmartWarehouse.API.Entities;

/// <summary>
/// Ürün tanım kartı. Stokta tutulacak ürünlerin master veri kaydı.
/// SKU şirket bazında unique olmalıdır.
/// </summary>
public class Product : BaseEntity
{
    /// <summary>
    /// Stock Keeping Unit - Şirket içinde benzersiz ürün kodu
    /// </summary>
    public string SKU { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    /// <summary>
    /// Ölçü birimi: Adet, Kg, Litre, Kutu, Palet
    /// </summary>
    public string Unit { get; set; } = "Adet";

    /// <summary>
    /// Barkod numarası (opsiyonel, EAN-13 vb.)
    /// </summary>
    public string? Barcode { get; set; }

    public decimal UnitCost { get; set; }
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// Kritik stok seviyesi. Bu seviyenin altına düşünce uyarı verilir.
    /// </summary>
    public int MinStockLevel { get; set; } = 0;

    /// <summary>
    /// Maksimum stok seviyesi. Depoyu taşırmamak için üst sınır.
    /// </summary>
    public int MaxStockLevel { get; set; } = 9999;

    /// <summary>
    /// Ürünün aktif satışta/kullanımda olup olmadığı
    /// </summary>
    public bool IsActive { get; set; } = true;

    // Foreign Key
    public Guid ProductCategoryId { get; set; }

    // Navigation Properties
    public ProductCategory Category { get; set; } = null!;
    public ICollection<WarehouseStock> Stocks { get; set; } = new List<WarehouseStock>();
    public ICollection<InventoryTransaction> Transactions { get; set; } = new List<InventoryTransaction>();
}
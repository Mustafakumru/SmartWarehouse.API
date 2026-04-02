// Entities/InventoryTransaction.cs
namespace SmartWarehouse.API.Entities;

/// <summary>
/// Stok Hareket Kaydı. Depoya her giriş ve çıkış bu tabloya yazılır.
/// Geçmişe dönük izleme (audit trail) sağlar.
/// Hiçbir kayıt fiziksel olarak silinmez.
/// </summary>
public class InventoryTransaction : BaseEntity
{
    /// <summary>
    /// Otomatik üretilen işlem kodu. Örn: "TXN-20240115-0001"
    /// </summary>
    public string TransactionCode { get; set; } = string.Empty;

    public Guid ProductId { get; set; }
    public Guid WarehouseRackId { get; set; }

    /// <summary>
    /// Hareket tipi:
    /// IN  = Depoya Giriş (Satın alma, üretim vb.)
    /// OUT = Depodan Çıkış (Satış, kullanım vb.)
    /// ADJ = Sayım Düzeltmesi (fazla/eksik)
    /// TRF = Transfer (raf değişikliği)
    /// </summary>
    public TransactionType TransactionType { get; set; }

    /// <summary>
    /// Hareket miktarı. Her zaman pozitif girilir.
    /// TransactionType'a göre stok artırılır veya azaltılır.
    /// </summary>
    public int Quantity { get; set; }

    public decimal UnitCost { get; set; }

    /// <summary>
    /// Toplam maliyet = Quantity * UnitCost
    /// </summary>
    public decimal TotalCost => Quantity * UnitCost;

    /// <summary>
    /// Referans numarası: Sipariş no, irsaliye no vb.
    /// </summary>
    public string? ReferenceNumber { get; set; }

    public string? Notes { get; set; }

    /// <summary>
    /// İşlemi gerçekleştiren kullanıcı adı veya ID
    /// </summary>
    public string CreatedBy { get; set; } = string.Empty;

    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// İşlem sonrası o raftaki anlık stok miktarı (snapshot)
    /// Raporlama ve denetim kolaylığı için tutulur.
    /// </summary>
    public int StockAfterTransaction { get; set; }

    // Navigation Properties
    public Product Product { get; set; } = null!;
    public WarehouseRack WarehouseRack { get; set; } = null!;
}

/// <summary>
/// Stok hareket türleri
/// </summary>
public enum TransactionType
{
    /// <summary>Depoya Giriş</summary>
    IN = 1,
    /// <summary>Depodan Çıkış</summary>
    OUT = 2,
    /// <summary>Sayım Düzeltmesi</summary>
    ADJ = 3,
    /// <summary>Raf Transferi</summary>
    TRF = 4
}
// Entities/BaseEntity.cs
namespace SmartWarehouse.API.Entities;

/// <summary>
/// Tüm entity'lerin miras aldığı temel sınıf.
/// CompanyId multi-tenant yapıyı, IsDeleted soft-delete'i sağlar.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Multi-tenant yapı için zorunlu alan.
    /// Her kayıt hangi şirkete ait olduğunu bilmek zorundadır.
    /// </summary>
    public string CompanyId { get; set; } = string.Empty;

    /// <summary>
    /// Soft-delete bayrağı. true ise kayıt silinmiş kabul edilir,
    /// fiziksel olarak DB'den kaldırılmaz.
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
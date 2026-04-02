// Entities/ProductCategory.cs
namespace SmartWarehouse.API.Entities;

/// <summary>
/// Ürün kategorileri. Elektronik, Gıda, Tekstil vb.
/// Renk kodu ile UI'da görsel ayrım sağlanır.
/// </summary>
public class ProductCategory : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    /// <summary>
    /// UI'da kategori kartları için renk kodu. Örn: "#FF5733"
    /// </summary>
    public string ColorCode { get; set; } = "#607D8B";

    public string? IconName { get; set; }

    // Navigation Property
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
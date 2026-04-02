namespace SmartWarehouse.API.DTOs.ProductCategory
{
    /// <summary>
    /// POST ile soft-delete yapılan tüm endpoint'lerde kullanılan
    /// ortak request body. DELETE HTTP metodu kullanılmaz.
    /// </summary>
    // Sadece bu iki alanı taşır, başka bir şey değil
    public class DeleteRequestDto
    {
        public Guid Id { get; set; }
        public string CompanyId { get; set; } = string.Empty;
    }
}

// Repositories/Interfaces/IBaseRepository.cs
namespace SmartWarehouse.API.Repositories.Interfaces;

/// <summary>
/// Tüm repository'lerin implement edeceği generic arayüz.
/// CompanyId her metotta zorunludur — multi-tenant güvenlik katmanı.
/// </summary>
public interface IBaseRepository<TEntity> where TEntity : class
{
    Task<TEntity?> GetByIdAsync(Guid id, string companyId);
    Task<IEnumerable<TEntity>> GetAllAsync(string companyId);
    Task<TEntity> CreateAsync(TEntity entity);
    Task UpdateAsync(TEntity entity);

    /// <summary>
    /// Fiziksel silme YOKTUR. Bu metod IsDeleted = true yapar.
    /// </summary>
    Task SoftDeleteAsync(Guid id, string companyId);

    Task<bool> ExistsAsync(Guid id, string companyId);
}
// Repositories/Interfaces/IProductCategoryRepository.cs
using SmartWarehouse.API.Entities;

namespace SmartWarehouse.API.Repositories.Interfaces;

public interface IProductCategoryRepository : IBaseRepository<ProductCategory>
{
    Task<bool> IsNameUniqueAsync(string name, string companyId, Guid? excludeId = null);
    Task<IEnumerable<ProductCategory>> GetActiveCategoriesAsync(string companyId);
}
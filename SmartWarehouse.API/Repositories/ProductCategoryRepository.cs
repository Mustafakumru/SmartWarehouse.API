// Repositories/ProductCategoryRepository.cs
using Microsoft.EntityFrameworkCore;
using SmartWarehouse.API.Data;
using SmartWarehouse.API.Entities;
using SmartWarehouse.API.Repositories.Interfaces;

namespace SmartWarehouse.API.Repositories;

public class ProductCategoryRepository
    : BaseRepository<ProductCategory>, IProductCategoryRepository
{
    public ProductCategoryRepository(SmartWarehouseDbContext context)
        : base(context) { }

    public async Task<bool> IsNameUniqueAsync(
        string name, string companyId, Guid? excludeId = null)
    {
        var query = _dbSet
            .Where(c => c.CompanyId == companyId && c.Name == name);

        if (excludeId.HasValue)
            query = query.Where(c => c.Id != excludeId.Value);

        return !await query.AnyAsync();
    }

    public async Task<IEnumerable<ProductCategory>> GetActiveCategoriesAsync(string companyId)
    {
        return await _dbSet
            .Where(c => c.CompanyId == companyId)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }
}
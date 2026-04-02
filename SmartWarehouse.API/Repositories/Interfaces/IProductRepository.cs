// Repositories/Interfaces/IProductRepository.cs
using SmartWarehouse.API.Entities;

namespace SmartWarehouse.API.Repositories.Interfaces;

public interface IProductRepository : IBaseRepository<Product>
{
    Task<bool> IsSkuUniqueAsync(string sku, string companyId, Guid? excludeId = null);

    /// <summary>
    /// Server-side pagination + arama + filtreleme
    /// </summary>
    Task<(IEnumerable<Product> Items, int TotalCount)> GetPagedAsync(
        string companyId,
        int pageNumber,
        int pageSize,
        string? searchTerm,
        Guid? categoryId,
        bool? isActive);

    Task<Product?> GetProductWithCategoryAsync(Guid productId, string companyId);
    Task<IEnumerable<Product>> GetActiveProductsAsync(string companyId);
    Task<IEnumerable<Product>> GetLowStockProductsAsync(string companyId);
}
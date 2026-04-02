// Repositories/ProductRepository.cs
using Microsoft.EntityFrameworkCore;
using SmartWarehouse.API.Data;
using SmartWarehouse.API.Entities;
using SmartWarehouse.API.Repositories.Interfaces;

namespace SmartWarehouse.API.Repositories;

public class ProductRepository
    : BaseRepository<Product>, IProductRepository
{
    public ProductRepository(SmartWarehouseDbContext context)
        : base(context) { }

    public async Task<bool> IsSkuUniqueAsync(
        string sku, string companyId, Guid? excludeId = null)
    {
        var query = _dbSet
            .Where(p => p.CompanyId == companyId && p.SKU == sku);

        if (excludeId.HasValue)
            query = query.Where(p => p.Id != excludeId.Value);

        return !await query.AnyAsync();
    }

    public async Task<(IEnumerable<Product> Items, int TotalCount)> GetPagedAsync(
        string companyId,
        int pageNumber,
        int pageSize,
        string? searchTerm,
        Guid? categoryId,
        bool? isActive)
    {
        // Tüm filtreler DB tarafında uygulanır — client-side pagination YOK
        var query = _dbSet
            .Include(p => p.Category)
            .Where(p => p.CompanyId == companyId);

        // Arama: SKU, Ad veya Barkod'da arar
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();
            query = query.Where(p =>
                p.Name.ToLower().Contains(term) ||
                p.SKU.ToLower().Contains(term) ||
                (p.Barcode != null && p.Barcode.ToLower().Contains(term)));
        }

        // Kategori filtresi
        if (categoryId.HasValue)
            query = query.Where(p => p.ProductCategoryId == categoryId.Value);

        // Aktiflik filtresi
        if (isActive.HasValue)
            query = query.Where(p => p.IsActive == isActive.Value);

        // Önce toplam kayıt sayısını al (sayfalama için)
        var totalCount = await query.CountAsync();

        // Sonra sayfalı veriyi al
        var items = await query
            .OrderBy(p => p.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<Product?> GetProductWithCategoryAsync(Guid productId, string companyId)
    {
        return await _dbSet
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == productId && p.CompanyId == companyId);
    }

    public async Task<IEnumerable<Product>> GetActiveProductsAsync(string companyId)
    {
        return await _dbSet
            .Include(p => p.Category)
            .Where(p => p.CompanyId == companyId && p.IsActive)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Product>> GetLowStockProductsAsync(string companyId)
    {
        // Stok miktarı minimum seviyenin altında olan ürünler
        return await _dbSet
            .Include(p => p.Category)
            .Include(p => p.Stocks)
            .Where(p => p.CompanyId == companyId && p.IsActive &&
                        p.Stocks.Sum(s => s.Quantity) <= p.MinStockLevel)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }
}
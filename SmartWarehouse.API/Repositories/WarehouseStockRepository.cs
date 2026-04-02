// Repositories/WarehouseStockRepository.cs
using Microsoft.EntityFrameworkCore;
using SmartWarehouse.API.Data;
using SmartWarehouse.API.Entities;
using SmartWarehouse.API.Repositories.Interfaces;

namespace SmartWarehouse.API.Repositories;

public class WarehouseStockRepository
    : BaseRepository<WarehouseStock>, IWarehouseStockRepository
{
    public WarehouseStockRepository(SmartWarehouseDbContext context)
        : base(context) { }

    public async Task<WarehouseStock?> GetByProductAndRackAsync(
        Guid productId, Guid rackId, string companyId)
    {
        return await _dbSet
            .Include(s => s.Product)
            .Include(s => s.WarehouseRack)
            .FirstOrDefaultAsync(s =>
                s.ProductId == productId &&
                s.WarehouseRackId == rackId &&
                s.CompanyId == companyId);
    }

    public async Task<int> GetTotalStockByProductAsync(Guid productId, string companyId)
    {
        return await _dbSet
            .Where(s => s.ProductId == productId && s.CompanyId == companyId)
            .SumAsync(s => s.Quantity);
    }

    public async Task<IEnumerable<WarehouseStock>> GetStockByProductAsync(
        Guid productId, string companyId)
    {
        return await _dbSet
            .Include(s => s.WarehouseRack)
                .ThenInclude(r => r.WarehouseZone)
            .Where(s => s.ProductId == productId && s.CompanyId == companyId)
            .OrderBy(s => s.WarehouseRack.RackCode)
            .ToListAsync();
    }

    public async Task<IEnumerable<WarehouseStock>> GetStockByRackAsync(
        Guid rackId, string companyId)
    {
        return await _dbSet
            .Include(s => s.Product)
                .ThenInclude(p => p.Category)
            .Where(s => s.WarehouseRackId == rackId && s.CompanyId == companyId)
            .OrderBy(s => s.Product.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<WarehouseStock>> GetCriticalStocksAsync(string companyId)
    {
        return await _dbSet
            .Include(s => s.Product)
            .Include(s => s.WarehouseRack)
            .Where(s => s.CompanyId == companyId &&
                        s.Quantity <= s.Product.MinStockLevel)
            .OrderBy(s => s.Quantity)
            .ToListAsync();
    }
}
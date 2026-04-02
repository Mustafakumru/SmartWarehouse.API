// Repositories/WarehouseRackRepository.cs
using Microsoft.EntityFrameworkCore;
using SmartWarehouse.API.Data;
using SmartWarehouse.API.Entities;
using SmartWarehouse.API.Repositories.Interfaces;

namespace SmartWarehouse.API.Repositories;

public class WarehouseRackRepository
    : BaseRepository<WarehouseRack>, IWarehouseRackRepository
{
    public WarehouseRackRepository(SmartWarehouseDbContext context)
        : base(context) { }

    public async Task<bool> IsRackCodeUniqueAsync(
        string rackCode, string companyId, Guid? excludeId = null)
    {
        var query = _dbSet
            .Where(r => r.CompanyId == companyId && r.RackCode == rackCode);

        if (excludeId.HasValue)
            query = query.Where(r => r.Id != excludeId.Value);

        return !await query.AnyAsync();
    }

    public async Task<IEnumerable<WarehouseRack>> GetRacksByZoneAsync(
        Guid zoneId, string companyId)
    {
        return await _dbSet
            .Include(r => r.WarehouseZone)
            .Where(r => r.CompanyId == companyId && r.WarehouseZoneId == zoneId)
            .OrderBy(r => r.RackCode)
            .ToListAsync();
    }

    public async Task<IEnumerable<WarehouseRack>> GetActiveRacksAsync(string companyId)
    {
        return await _dbSet
            .Include(r => r.WarehouseZone)
            .Where(r => r.CompanyId == companyId && r.IsActive)
            .OrderBy(r => r.RackCode)
            .ToListAsync();
    }
}
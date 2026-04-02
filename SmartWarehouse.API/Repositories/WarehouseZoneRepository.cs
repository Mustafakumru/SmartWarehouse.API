// Repositories/WarehouseZoneRepository.cs
using Microsoft.EntityFrameworkCore;
using SmartWarehouse.API.Data;
using SmartWarehouse.API.Entities;
using SmartWarehouse.API.Repositories.Interfaces;

namespace SmartWarehouse.API.Repositories;

public class WarehouseZoneRepository
    : BaseRepository<WarehouseZone>, IWarehouseZoneRepository
{
    public WarehouseZoneRepository(SmartWarehouseDbContext context)
        : base(context) { }

    public async Task<bool> IsZoneCodeUniqueAsync(
        string zoneCode, string companyId, Guid? excludeId = null)
    {
        var query = _dbSet
            .Where(z => z.CompanyId == companyId && z.ZoneCode == zoneCode);

        if (excludeId.HasValue)
            query = query.Where(z => z.Id != excludeId.Value);

        return !await query.AnyAsync();
    }

    public async Task<IEnumerable<WarehouseZone>> GetActiveZonesAsync(string companyId)
    {
        return await _dbSet
            .Where(z => z.CompanyId == companyId && z.IsActive)
            .OrderBy(z => z.ZoneCode)
            .ToListAsync();
    }

    public async Task<WarehouseZone?> GetZoneWithRacksAsync(Guid zoneId, string companyId)
    {
        return await _dbSet
            .Include(z => z.Racks.Where(r => !r.IsDeleted))
            .FirstOrDefaultAsync(z => z.Id == zoneId && z.CompanyId == companyId);
    }
}
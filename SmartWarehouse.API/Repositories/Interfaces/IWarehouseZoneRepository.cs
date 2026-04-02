// Repositories/Interfaces/IWarehouseZoneRepository.cs
using SmartWarehouse.API.Entities;

namespace SmartWarehouse.API.Repositories.Interfaces;

public interface IWarehouseZoneRepository : IBaseRepository<WarehouseZone>
{
    Task<bool> IsZoneCodeUniqueAsync(string zoneCode, string companyId, Guid? excludeId = null);
    Task<IEnumerable<WarehouseZone>> GetActiveZonesAsync(string companyId);
    Task<WarehouseZone?> GetZoneWithRacksAsync(Guid zoneId, string companyId);
}
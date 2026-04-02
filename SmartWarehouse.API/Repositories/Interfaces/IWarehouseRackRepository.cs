// Repositories/Interfaces/IWarehouseRackRepository.cs
using SmartWarehouse.API.Entities;

namespace SmartWarehouse.API.Repositories.Interfaces;

public interface IWarehouseRackRepository : IBaseRepository<WarehouseRack>
{
    Task<bool> IsRackCodeUniqueAsync(string rackCode, string companyId, Guid? excludeId = null);
    Task<IEnumerable<WarehouseRack>> GetRacksByZoneAsync(Guid zoneId, string companyId);
    Task<IEnumerable<WarehouseRack>> GetActiveRacksAsync(string companyId);
}
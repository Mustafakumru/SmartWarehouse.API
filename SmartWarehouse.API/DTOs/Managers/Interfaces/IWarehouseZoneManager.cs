// Managers/Interfaces/IWarehouseZoneManager.cs
using SmartWarehouse.API.DTOs.Common;
using SmartWarehouse.API.DTOs.WarehouseZone;

namespace SmartWarehouse.API.Managers.Interfaces;

public interface IWarehouseZoneManager
{
    Task<ApiResponseDto<IEnumerable<WarehouseZoneDto>>> GetAllAsync(string companyId);
    Task<ApiResponseDto<WarehouseZoneDto>> GetByIdAsync(Guid id, string companyId);
    Task<ApiResponseDto<WarehouseZoneDto>> CreateAsync(CreateWarehouseZoneDto dto);
    Task<ApiResponseDto<WarehouseZoneDto>> UpdateAsync(UpdateWarehouseZoneDto dto);
    Task<ApiResponseDto<bool>> DeleteAsync(Guid id, string companyId);
}
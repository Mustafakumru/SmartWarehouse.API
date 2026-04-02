// Managers/Interfaces/IWarehouseRackManager.cs
using SmartWarehouse.API.DTOs.Common;
using SmartWarehouse.API.DTOs.WarehouseRack;

namespace SmartWarehouse.API.Managers.Interfaces;

public interface IWarehouseRackManager
{
    Task<ApiResponseDto<IEnumerable<WarehouseRackDto>>> GetAllAsync(string companyId);
    Task<ApiResponseDto<WarehouseRackDto>> GetByIdAsync(Guid id, string companyId);
    Task<ApiResponseDto<IEnumerable<WarehouseRackDto>>> GetByZoneAsync(Guid zoneId, string companyId);
    Task<ApiResponseDto<WarehouseRackDto>> CreateAsync(CreateWarehouseRackDto dto);
    Task<ApiResponseDto<WarehouseRackDto>> UpdateAsync(UpdateWarehouseRackDto dto);
    Task<ApiResponseDto<bool>> DeleteAsync(Guid id, string companyId);
}
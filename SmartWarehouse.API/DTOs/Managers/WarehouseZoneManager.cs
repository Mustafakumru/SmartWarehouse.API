// Managers/WarehouseZoneManager.cs
using SmartWarehouse.API.DTOs.Common;
using SmartWarehouse.API.DTOs.WarehouseZone;
using SmartWarehouse.API.Entities;
using SmartWarehouse.API.Managers.Interfaces;
using SmartWarehouse.API.Repositories.Interfaces;

namespace SmartWarehouse.API.Managers;

public class WarehouseZoneManager : IWarehouseZoneManager
{
    private readonly IWarehouseZoneRepository _zoneRepository;

    public WarehouseZoneManager(IWarehouseZoneRepository zoneRepository)
    {
        _zoneRepository = zoneRepository;
    }

    public async Task<ApiResponseDto<IEnumerable<WarehouseZoneDto>>> GetAllAsync(
        string companyId)
    {
        if (string.IsNullOrWhiteSpace(companyId))
            return ApiResponseDto<IEnumerable<WarehouseZoneDto>>.Fail("CompanyId boş olamaz.");

        var zones = await _zoneRepository.GetAllAsync(companyId);
        var dtos = zones.Select(MapToDto);
        return ApiResponseDto<IEnumerable<WarehouseZoneDto>>.Ok(dtos);
    }

    public async Task<ApiResponseDto<WarehouseZoneDto>> GetByIdAsync(
        Guid id, string companyId)
    {
        if (string.IsNullOrWhiteSpace(companyId))
            return ApiResponseDto<WarehouseZoneDto>.Fail("CompanyId boş olamaz.");

        var entity = await _zoneRepository.GetZoneWithRacksAsync(id, companyId);
        if (entity is null)
            return ApiResponseDto<WarehouseZoneDto>.Fail("Bölge bulunamadı.");

        return ApiResponseDto<WarehouseZoneDto>.Ok(MapToDto(entity));
    }

    public async Task<ApiResponseDto<WarehouseZoneDto>> CreateAsync(
        CreateWarehouseZoneDto dto)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(dto.CompanyId))
            errors.Add("CompanyId zorunludur.");

        if (string.IsNullOrWhiteSpace(dto.ZoneCode))
            errors.Add("Bölge kodu zorunludur.");

        if (string.IsNullOrWhiteSpace(dto.ZoneName))
            errors.Add("Bölge adı zorunludur.");

        if (dto.MaxCapacity <= 0)
            errors.Add("Kapasite 0'dan büyük olmalıdır.");

        if (errors.Any())
            return ApiResponseDto<WarehouseZoneDto>.Fail("Validasyon hatası.", errors);

        var isUnique = await _zoneRepository
            .IsZoneCodeUniqueAsync(dto.ZoneCode!, dto.CompanyId);
        if (!isUnique)
            return ApiResponseDto<WarehouseZoneDto>
                .Fail($"'{dto.ZoneCode}' kodlu bölge zaten mevcut.");

        var entity = new WarehouseZone
        {
            CompanyId = dto.CompanyId,
            ZoneCode = dto.ZoneCode!.ToUpper().Trim(),
            ZoneName = dto.ZoneName!,
            Description = dto.Description,
            MaxCapacity = dto.MaxCapacity,
            TemperatureRequirement = dto.TemperatureRequirement,
            IsActive = true
        };

        var created = await _zoneRepository.CreateAsync(entity);
        return ApiResponseDto<WarehouseZoneDto>
            .Ok(MapToDto(created), "Depo bölgesi başarıyla oluşturuldu.");
    }

    public async Task<ApiResponseDto<WarehouseZoneDto>> UpdateAsync(
        UpdateWarehouseZoneDto dto)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(dto.CompanyId))
            errors.Add("CompanyId zorunludur.");

        if (dto.Id == Guid.Empty)
            errors.Add("Geçerli bir Id zorunludur.");

        if (string.IsNullOrWhiteSpace(dto.ZoneCode))
            errors.Add("Bölge kodu zorunludur.");

        if (dto.MaxCapacity <= 0)
            errors.Add("Kapasite 0'dan büyük olmalıdır.");

        if (errors.Any())
            return ApiResponseDto<WarehouseZoneDto>.Fail("Validasyon hatası.", errors);

        var entity = await _zoneRepository.GetByIdAsync(dto.Id, dto.CompanyId);
        if (entity is null)
            return ApiResponseDto<WarehouseZoneDto>
                .Fail("Bölge bulunamadı veya bu şirkete ait değil.");

        if (!entity.ZoneCode.Equals(dto.ZoneCode, StringComparison.OrdinalIgnoreCase))
        {
            var isUnique = await _zoneRepository
                .IsZoneCodeUniqueAsync(dto.ZoneCode!, dto.CompanyId, dto.Id);
            if (!isUnique)
                return ApiResponseDto<WarehouseZoneDto>
                    .Fail($"'{dto.ZoneCode}' kodlu bölge zaten mevcut.");
        }

        entity.ZoneCode = dto.ZoneCode!.ToUpper().Trim();
        entity.ZoneName = dto.ZoneName;
        entity.Description = dto.Description;
        entity.MaxCapacity = dto.MaxCapacity;
        entity.TemperatureRequirement = dto.TemperatureRequirement;
        entity.IsActive = dto.IsActive;

        await _zoneRepository.UpdateAsync(entity);
        return ApiResponseDto<WarehouseZoneDto>
            .Ok(MapToDto(entity), "Depo bölgesi başarıyla güncellendi.");
    }

    public async Task<ApiResponseDto<bool>> DeleteAsync(Guid id, string companyId)
    {
        if (string.IsNullOrWhiteSpace(companyId))
            return ApiResponseDto<bool>.Fail("CompanyId zorunludur.");

        var exists = await _zoneRepository.ExistsAsync(id, companyId);
        if (!exists)
            return ApiResponseDto<bool>.Fail("Bölge bulunamadı veya bu şirkete ait değil.");

        await _zoneRepository.SoftDeleteAsync(id, companyId);
        return ApiResponseDto<bool>.Ok(true, "Depo bölgesi başarıyla silindi.");
    }

    private static WarehouseZoneDto MapToDto(WarehouseZone entity) => new()
    {
        Id = entity.Id,
        CompanyId = entity.CompanyId,
        ZoneCode = entity.ZoneCode,
        ZoneName = entity.ZoneName,
        Description = entity.Description,
        MaxCapacity = entity.MaxCapacity,
        TemperatureRequirement = entity.TemperatureRequirement,
        IsActive = entity.IsActive,
        RackCount = entity.Racks?.Count(r => !r.IsDeleted) ?? 0,
        CreatedAt = entity.CreatedAt
    };
}
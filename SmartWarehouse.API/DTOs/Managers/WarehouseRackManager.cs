// Managers/WarehouseRackManager.cs
using SmartWarehouse.API.DTOs.Common;
using SmartWarehouse.API.DTOs.WarehouseRack;
using SmartWarehouse.API.Entities;
using SmartWarehouse.API.Managers.Interfaces;
using SmartWarehouse.API.Repositories.Interfaces;

namespace SmartWarehouse.API.Managers;

public class WarehouseRackManager : IWarehouseRackManager
{
    private readonly IWarehouseRackRepository _rackRepository;
    private readonly IWarehouseZoneRepository _zoneRepository;

    public WarehouseRackManager(
        IWarehouseRackRepository rackRepository,
        IWarehouseZoneRepository zoneRepository)
    {
        _rackRepository = rackRepository;
        _zoneRepository = zoneRepository;
    }

    public async Task<ApiResponseDto<IEnumerable<WarehouseRackDto>>> GetAllAsync(
        string companyId)
    {
        if (string.IsNullOrWhiteSpace(companyId))
            return ApiResponseDto<IEnumerable<WarehouseRackDto>>.Fail("CompanyId boş olamaz.");

        var racks = await _rackRepository.GetActiveRacksAsync(companyId);
        return ApiResponseDto<IEnumerable<WarehouseRackDto>>.Ok(racks.Select(MapToDto));
    }

    public async Task<ApiResponseDto<WarehouseRackDto>> GetByIdAsync(
        Guid id, string companyId)
    {
        if (string.IsNullOrWhiteSpace(companyId))
            return ApiResponseDto<WarehouseRackDto>.Fail("CompanyId boş olamaz.");

        var entity = await _rackRepository.GetByIdAsync(id, companyId);
        if (entity is null)
            return ApiResponseDto<WarehouseRackDto>.Fail("Raf bulunamadı.");

        return ApiResponseDto<WarehouseRackDto>.Ok(MapToDto(entity));
    }

    public async Task<ApiResponseDto<IEnumerable<WarehouseRackDto>>> GetByZoneAsync(
        Guid zoneId, string companyId)
    {
        if (string.IsNullOrWhiteSpace(companyId))
            return ApiResponseDto<IEnumerable<WarehouseRackDto>>.Fail("CompanyId boş olamaz.");

        var racks = await _rackRepository.GetRacksByZoneAsync(zoneId, companyId);
        return ApiResponseDto<IEnumerable<WarehouseRackDto>>.Ok(racks.Select(MapToDto));
    }

    public async Task<ApiResponseDto<WarehouseRackDto>> CreateAsync(
        CreateWarehouseRackDto dto)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(dto.CompanyId))
            errors.Add("CompanyId zorunludur.");

        if (string.IsNullOrWhiteSpace(dto.RackCode))
            errors.Add("Raf kodu zorunludur.");

        if (string.IsNullOrWhiteSpace(dto.RackName))
            errors.Add("Raf adı zorunludur.");

        if (dto.MaxCapacity <= 0)
            errors.Add("Kapasite 0'dan büyük olmalıdır.");

        if (dto.WarehouseZoneId == Guid.Empty)
            errors.Add("Geçerli bir Bölge seçilmelidir.");

        if (errors.Any())
            return ApiResponseDto<WarehouseRackDto>.Fail("Validasyon hatası.", errors);

        // Bölgenin bu şirkete ait olup olmadığını kontrol et
        var zoneExists = await _zoneRepository.ExistsAsync(dto.WarehouseZoneId, dto.CompanyId);
        if (!zoneExists)
            return ApiResponseDto<WarehouseRackDto>
                .Fail("Seçilen bölge bulunamadı veya bu şirkete ait değil.");

        var isUnique = await _rackRepository
            .IsRackCodeUniqueAsync(dto.RackCode!, dto.CompanyId);
        if (!isUnique)
            return ApiResponseDto<WarehouseRackDto>
                .Fail($"'{dto.RackCode}' kodlu raf zaten mevcut.");

        var entity = new WarehouseRack
        {
            CompanyId = dto.CompanyId,
            RackCode = dto.RackCode!.ToUpper().Trim(),
            RackName = dto.RackName!,
            MaxCapacity = dto.MaxCapacity,
            RackType = string.IsNullOrWhiteSpace(dto.RackType) ? "Standard" : dto.RackType,
            WarehouseZoneId = dto.WarehouseZoneId,
            IsActive = true
        };

        var created = await _rackRepository.CreateAsync(entity);

        // Zone bilgisini yükle
        var entityWithZone = await _rackRepository.GetByIdAsync(created.Id, dto.CompanyId);
        return ApiResponseDto<WarehouseRackDto>
            .Ok(MapToDto(entityWithZone!), "Raf başarıyla oluşturuldu.");
    }

    public async Task<ApiResponseDto<WarehouseRackDto>> UpdateAsync(
        UpdateWarehouseRackDto dto)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(dto.CompanyId))
            errors.Add("CompanyId zorunludur.");

        if (dto.Id == Guid.Empty)
            errors.Add("Geçerli bir Id zorunludur.");

        if (string.IsNullOrWhiteSpace(dto.RackCode))
            errors.Add("Raf kodu zorunludur.");

        if (dto.MaxCapacity <= 0)
            errors.Add("Kapasite 0'dan büyük olmalıdır.");

        if (errors.Any())
            return ApiResponseDto<WarehouseRackDto>.Fail("Validasyon hatası.", errors);

        var entity = await _rackRepository.GetByIdAsync(dto.Id, dto.CompanyId);
        if (entity is null)
            return ApiResponseDto<WarehouseRackDto>
                .Fail("Raf bulunamadı veya bu şirkete ait değil.");

        if (!entity.RackCode.Equals(dto.RackCode, StringComparison.OrdinalIgnoreCase))
        {
            var isUnique = await _rackRepository
                .IsRackCodeUniqueAsync(dto.RackCode!, dto.CompanyId, dto.Id);
            if (!isUnique)
                return ApiResponseDto<WarehouseRackDto>
                    .Fail($"'{dto.RackCode}' kodlu raf zaten mevcut.");
        }

        entity.RackCode = dto.RackCode!.ToUpper().Trim();
        entity.RackName = dto.RackName;
        entity.MaxCapacity = dto.MaxCapacity;
        entity.RackType = dto.RackType;
        entity.IsActive = dto.IsActive;
        entity.WarehouseZoneId = dto.WarehouseZoneId;

        await _rackRepository.UpdateAsync(entity);
        return ApiResponseDto<WarehouseRackDto>
            .Ok(MapToDto(entity), "Raf başarıyla güncellendi.");
    }

    public async Task<ApiResponseDto<bool>> DeleteAsync(Guid id, string companyId)
    {
        if (string.IsNullOrWhiteSpace(companyId))
            return ApiResponseDto<bool>.Fail("CompanyId zorunludur.");

        var exists = await _rackRepository.ExistsAsync(id, companyId);
        if (!exists)
            return ApiResponseDto<bool>.Fail("Raf bulunamadı veya bu şirkete ait değil.");

        await _rackRepository.SoftDeleteAsync(id, companyId);
        return ApiResponseDto<bool>.Ok(true, "Raf başarıyla silindi.");
    }

    private static WarehouseRackDto MapToDto(WarehouseRack entity) => new()
    {
        Id = entity.Id,
        CompanyId = entity.CompanyId,
        RackCode = entity.RackCode,
        RackName = entity.RackName,
        MaxCapacity = entity.MaxCapacity,
        RackType = entity.RackType,
        IsActive = entity.IsActive,
        WarehouseZoneId = entity.WarehouseZoneId,
        ZoneCode = entity.WarehouseZone?.ZoneCode ?? string.Empty,
        ZoneName = entity.WarehouseZone?.ZoneName ?? string.Empty,
        CreatedAt = entity.CreatedAt
    };
}
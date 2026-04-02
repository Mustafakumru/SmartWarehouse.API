// Managers/ProductCategoryManager.cs
using SmartWarehouse.API.DTOs.Common;
using SmartWarehouse.API.DTOs.ProductCategory;
using SmartWarehouse.API.Entities;
using SmartWarehouse.API.Managers.Interfaces;
using SmartWarehouse.API.Repositories.Interfaces;

namespace SmartWarehouse.API.Managers;

public class ProductCategoryManager : IProductCategoryManager
{
    private readonly IProductCategoryRepository _categoryRepository;

    public ProductCategoryManager(IProductCategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<ApiResponseDto<IEnumerable<ProductCategoryDto>>> GetAllAsync(
        string companyId)
    {
        if (string.IsNullOrWhiteSpace(companyId))
            return ApiResponseDto<IEnumerable<ProductCategoryDto>>
                .Fail("CompanyId boş olamaz.");

        var categories = await _categoryRepository.GetActiveCategoriesAsync(companyId);
        var dtos = categories.Select(MapToDto);
        return ApiResponseDto<IEnumerable<ProductCategoryDto>>.Ok(dtos);
    }

    public async Task<ApiResponseDto<ProductCategoryDto>> GetByIdAsync(
        Guid id, string companyId)
    {
        if (string.IsNullOrWhiteSpace(companyId))
            return ApiResponseDto<ProductCategoryDto>.Fail("CompanyId boş olamaz.");

        var entity = await _categoryRepository.GetByIdAsync(id, companyId);
        if (entity is null)
            return ApiResponseDto<ProductCategoryDto>.Fail("Kategori bulunamadı.");

        return ApiResponseDto<ProductCategoryDto>.Ok(MapToDto(entity));
    }

    public async Task<ApiResponseDto<ProductCategoryDto>> CreateAsync(
        CreateProductCategoryDto dto)
    {
        // ── Validasyon ────────────────────────────────────────────────
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(dto.CompanyId))
            errors.Add("CompanyId zorunludur.");

        if (string.IsNullOrWhiteSpace(dto.Name))
            errors.Add("Kategori adı zorunludur.");

        if (dto.Name?.Length > 100)
            errors.Add("Kategori adı en fazla 100 karakter olabilir.");

        if (errors.Any())
            return ApiResponseDto<ProductCategoryDto>.Fail("Validasyon hatası.", errors);

        // ── İş Kuralı: Aynı isimde kategori olmamalı ─────────────────
        var isUnique = await _categoryRepository.IsNameUniqueAsync(dto.Name!, dto.CompanyId);
        if (!isUnique)
            return ApiResponseDto<ProductCategoryDto>
                .Fail($"'{dto.Name}' adında bir kategori zaten mevcut.");

        var entity = new ProductCategory
        {
            CompanyId = dto.CompanyId,
            Name = dto.Name!,
            Description = dto.Description,
            ColorCode = string.IsNullOrWhiteSpace(dto.ColorCode) ? "#607D8B" : dto.ColorCode,
            IconName = dto.IconName
        };

        var created = await _categoryRepository.CreateAsync(entity);
        return ApiResponseDto<ProductCategoryDto>
            .Ok(MapToDto(created), "Kategori başarıyla oluşturuldu.");
    }

    public async Task<ApiResponseDto<ProductCategoryDto>> UpdateAsync(
        UpdateProductCategoryDto dto)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(dto.CompanyId))
            errors.Add("CompanyId zorunludur.");

        if (dto.Id == Guid.Empty)
            errors.Add("Geçerli bir Id zorunludur.");

        if (string.IsNullOrWhiteSpace(dto.Name))
            errors.Add("Kategori adı zorunludur.");

        if (errors.Any())
            return ApiResponseDto<ProductCategoryDto>.Fail("Validasyon hatası.", errors);

        // CompanyId eşleşme kontrolü
        var entity = await _categoryRepository.GetByIdAsync(dto.Id, dto.CompanyId);
        if (entity is null)
            return ApiResponseDto<ProductCategoryDto>
                .Fail("Kategori bulunamadı veya bu şirkete ait değil.");

        // İsim değiştiyse unique kontrolü
        if (!entity.Name.Equals(dto.Name, StringComparison.OrdinalIgnoreCase))
        {
            var isUnique = await _categoryRepository
                .IsNameUniqueAsync(dto.Name!, dto.CompanyId, dto.Id);
            if (!isUnique)
                return ApiResponseDto<ProductCategoryDto>
                    .Fail($"'{dto.Name}' adında bir kategori zaten mevcut.");
        }

        // Alanları güncelle
        entity.Name = dto.Name!;
        entity.Description = dto.Description;
        entity.ColorCode = string.IsNullOrWhiteSpace(dto.ColorCode) ? "#607D8B" : dto.ColorCode;
        entity.IconName = dto.IconName;

        // KIRMIZI ÇİZGİ: EntityState.Modified — BaseRepository.UpdateAsync içinde uygulanır
        await _categoryRepository.UpdateAsync(entity);
        return ApiResponseDto<ProductCategoryDto>
            .Ok(MapToDto(entity), "Kategori başarıyla güncellendi.");
    }

    public async Task<ApiResponseDto<bool>> DeleteAsync(Guid id, string companyId)
    {
        if (string.IsNullOrWhiteSpace(companyId))
            return ApiResponseDto<bool>.Fail("CompanyId zorunludur.");

        var exists = await _categoryRepository.ExistsAsync(id, companyId);
        if (!exists)
            return ApiResponseDto<bool>.Fail("Kategori bulunamadı veya bu şirkete ait değil.");

        // KIRMIZI ÇİZGİ: Soft delete — IsDeleted = true
        await _categoryRepository.SoftDeleteAsync(id, companyId);
        return ApiResponseDto<bool>.Ok(true, "Kategori başarıyla silindi.");
    }

    // ── Mapper ───────────────────────────────────────────────────────────────
    private static ProductCategoryDto MapToDto(ProductCategory entity) => new()
    {
        Id = entity.Id,
        CompanyId = entity.CompanyId,
        Name = entity.Name,
        Description = entity.Description,
        ColorCode = entity.ColorCode,
        IconName = entity.IconName,
        ProductCount = entity.Products?.Count(p => !p.IsDeleted) ?? 0,
        CreatedAt = entity.CreatedAt
    };
}
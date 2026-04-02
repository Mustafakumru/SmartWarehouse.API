// Managers/ProductManager.cs
using SmartWarehouse.API.DTOs.Common;
using SmartWarehouse.API.DTOs.Product;
using SmartWarehouse.API.Entities;
using SmartWarehouse.API.Managers.Interfaces;
using SmartWarehouse.API.Repositories.Interfaces;

namespace SmartWarehouse.API.Managers;

public class ProductManager : IProductManager
{
    private readonly IProductRepository _productRepository;
    private readonly IProductCategoryRepository _categoryRepository;
    private readonly IWarehouseStockRepository _stockRepository;

    public ProductManager(
        IProductRepository productRepository,
        IProductCategoryRepository categoryRepository,
        IWarehouseStockRepository stockRepository)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _stockRepository = stockRepository;
    }

    public async Task<ApiResponseDto<PagedResponseDto<ProductListDto>>> GetPagedAsync(
        ProductFilterDto filter)
    {
        if (string.IsNullOrWhiteSpace(filter.CompanyId))
            return ApiResponseDto<PagedResponseDto<ProductListDto>>
                .Fail("CompanyId zorunludur.");

        if (filter.PageNumber < 1) filter.PageNumber = 1;
        if (filter.PageSize < 1 || filter.PageSize > 100) filter.PageSize = 10;

        var (items, totalCount) = await _productRepository.GetPagedAsync(
            filter.CompanyId,
            filter.PageNumber,
            filter.PageSize,
            filter.SearchTerm,
            filter.CategoryId,
            filter.IsActive);

        // ✅ Task.WhenAll YERİNE sıralı foreach — DbContext thread-safe değil
        var dtos = new List<ProductListDto>();
        foreach (var p in items)
        {
            var totalStock = await _stockRepository
                .GetTotalStockByProductAsync(p.Id, filter.CompanyId);

            dtos.Add(new ProductListDto
            {
                Id = p.Id,
                SKU = p.SKU,
                Name = p.Name,
                Unit = p.Unit,
                UnitPrice = p.UnitPrice,
                IsActive = p.IsActive,
                CategoryName = p.Category?.Name ?? string.Empty,
                CategoryColorCode = p.Category?.ColorCode ?? "#607D8B",
                TotalStock = totalStock,
                MinStockLevel = p.MinStockLevel,
                IsLowStock = totalStock <= p.MinStockLevel
            });
        }

        var pagedResponse = new PagedResponseDto<ProductListDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            PageNumber = filter.PageNumber,
            PageSize = filter.PageSize
        };

        return ApiResponseDto<PagedResponseDto<ProductListDto>>.Ok(pagedResponse);
    }

    public async Task<ApiResponseDto<ProductDto>> GetByIdAsync(Guid id, string companyId)
    {
        if (string.IsNullOrWhiteSpace(companyId))
            return ApiResponseDto<ProductDto>.Fail("CompanyId zorunludur.");

        var entity = await _productRepository.GetProductWithCategoryAsync(id, companyId);
        if (entity is null)
            return ApiResponseDto<ProductDto>.Fail("Ürün bulunamadı.");

        var totalStock = await _stockRepository
            .GetTotalStockByProductAsync(id, companyId);

        return ApiResponseDto<ProductDto>.Ok(MapToDto(entity, totalStock));
    }

    public async Task<ApiResponseDto<ProductDto>> CreateAsync(CreateProductDto dto)
    {
        var errors = ValidateCreateProduct(dto);
        if (errors.Any())
            return ApiResponseDto<ProductDto>.Fail("Validasyon hatası.", errors);

        var isSkuUnique = await _productRepository
            .IsSkuUniqueAsync(dto.SKU, dto.CompanyId);
        if (!isSkuUnique)
            return ApiResponseDto<ProductDto>
                .Fail($"'{dto.SKU}' SKU kodu zaten kullanımda.");

        var categoryExists = await _categoryRepository
            .ExistsAsync(dto.ProductCategoryId, dto.CompanyId);
        if (!categoryExists)
            return ApiResponseDto<ProductDto>
                .Fail("Seçilen kategori bulunamadı veya bu şirkete ait değil.");

        var entity = new Product
        {
            CompanyId = dto.CompanyId,
            SKU = dto.SKU.ToUpper().Trim(),
            Name = dto.Name,
            Description = dto.Description,
            Unit = dto.Unit,
            Barcode = dto.Barcode,
            UnitCost = dto.UnitCost,
            UnitPrice = dto.UnitPrice,
            MinStockLevel = dto.MinStockLevel,
            MaxStockLevel = dto.MaxStockLevel,
            ProductCategoryId = dto.ProductCategoryId,
            IsActive = true
        };

        var created = await _productRepository.CreateAsync(entity);
        var withCategory = await _productRepository
            .GetProductWithCategoryAsync(created.Id, dto.CompanyId);

        return ApiResponseDto<ProductDto>
            .Ok(MapToDto(withCategory!, 0), "Ürün başarıyla oluşturuldu.");
    }

    public async Task<ApiResponseDto<ProductDto>> UpdateAsync(UpdateProductDto dto)
    {
        var errors = ValidateUpdateProduct(dto);
        if (errors.Any())
            return ApiResponseDto<ProductDto>.Fail("Validasyon hatası.", errors);

        var entity = await _productRepository
            .GetProductWithCategoryAsync(dto.Id, dto.CompanyId);
        if (entity is null)
            return ApiResponseDto<ProductDto>
                .Fail("Ürün bulunamadı veya bu şirkete ait değil.");

        if (!entity.SKU.Equals(dto.SKU, StringComparison.OrdinalIgnoreCase))
        {
            var isSkuUnique = await _productRepository
                .IsSkuUniqueAsync(dto.SKU, dto.CompanyId, dto.Id);
            if (!isSkuUnique)
                return ApiResponseDto<ProductDto>
                    .Fail($"'{dto.SKU}' SKU kodu zaten kullanımda.");
        }

        entity.SKU = dto.SKU.ToUpper().Trim();
        entity.Name = dto.Name;
        entity.Description = dto.Description;
        entity.Unit = dto.Unit;
        entity.Barcode = dto.Barcode;
        entity.UnitCost = dto.UnitCost;
        entity.UnitPrice = dto.UnitPrice;
        entity.MinStockLevel = dto.MinStockLevel;
        entity.MaxStockLevel = dto.MaxStockLevel;
        entity.IsActive = dto.IsActive;
        entity.ProductCategoryId = dto.ProductCategoryId;

        await _productRepository.UpdateAsync(entity);

        var totalStock = await _stockRepository
            .GetTotalStockByProductAsync(dto.Id, dto.CompanyId);

        return ApiResponseDto<ProductDto>
            .Ok(MapToDto(entity, totalStock), "Ürün başarıyla güncellendi.");
    }

    public async Task<ApiResponseDto<bool>> DeleteAsync(Guid id, string companyId)
    {
        if (string.IsNullOrWhiteSpace(companyId))
            return ApiResponseDto<bool>.Fail("CompanyId zorunludur.");

        var exists = await _productRepository.ExistsAsync(id, companyId);
        if (!exists)
            return ApiResponseDto<bool>.Fail("Ürün bulunamadı veya bu şirkete ait değil.");

        await _productRepository.SoftDeleteAsync(id, companyId);
        return ApiResponseDto<bool>.Ok(true, "Ürün başarıyla silindi.");
    }

    public async Task<ApiResponseDto<IEnumerable<ProductListDto>>> GetActiveProductsAsync(
        string companyId)
    {
        if (string.IsNullOrWhiteSpace(companyId))
            return ApiResponseDto<IEnumerable<ProductListDto>>
                .Fail("CompanyId zorunludur.");

        var products = await _productRepository.GetActiveProductsAsync(companyId);

        // ✅ Task.WhenAll YERİNE sıralı foreach
        var dtos = new List<ProductListDto>();
        foreach (var p in products)
        {
            var totalStock = await _stockRepository
                .GetTotalStockByProductAsync(p.Id, companyId);

            dtos.Add(new ProductListDto
            {
                Id = p.Id,
                SKU = p.SKU,
                Name = p.Name,
                Unit = p.Unit,
                UnitPrice = p.UnitPrice,
                IsActive = p.IsActive,
                CategoryName = p.Category?.Name ?? string.Empty,
                CategoryColorCode = p.Category?.ColorCode ?? "#607D8B",
                TotalStock = totalStock,
                MinStockLevel = p.MinStockLevel,
                IsLowStock = totalStock <= p.MinStockLevel
            });
        }

        return ApiResponseDto<IEnumerable<ProductListDto>>.Ok(dtos);
    }

    // ── Validasyonlar ─────────────────────────────────────────────────────────
    private static List<string> ValidateCreateProduct(CreateProductDto dto)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(dto.CompanyId))
            errors.Add("CompanyId zorunludur.");

        if (string.IsNullOrWhiteSpace(dto.SKU))
            errors.Add("SKU kodu zorunludur.");

        if (dto.SKU?.Length > 50)
            errors.Add("SKU en fazla 50 karakter olabilir.");

        if (string.IsNullOrWhiteSpace(dto.Name))
            errors.Add("Ürün adı zorunludur.");

        if (dto.Name?.Length > 200)
            errors.Add("Ürün adı en fazla 200 karakter olabilir.");

        if (dto.UnitCost < 0)
            errors.Add("Birim maliyet negatif olamaz.");

        if (dto.UnitPrice < 0)
            errors.Add("Birim fiyat negatif olamaz.");

        if (dto.MinStockLevel < 0)
            errors.Add("Minimum stok seviyesi negatif olamaz.");

        if (dto.MaxStockLevel < dto.MinStockLevel)
            errors.Add("Maksimum stok seviyesi minimumdan küçük olamaz.");

        if (dto.ProductCategoryId == Guid.Empty)
            errors.Add("Kategori seçimi zorunludur.");

        return errors;
    }

    private static List<string> ValidateUpdateProduct(UpdateProductDto dto)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(dto.CompanyId))
            errors.Add("CompanyId zorunludur.");

        if (dto.Id == Guid.Empty)
            errors.Add("Geçerli bir Id zorunludur.");

        if (string.IsNullOrWhiteSpace(dto.SKU))
            errors.Add("SKU kodu zorunludur.");

        if (string.IsNullOrWhiteSpace(dto.Name))
            errors.Add("Ürün adı zorunludur.");

        if (dto.UnitCost < 0)
            errors.Add("Birim maliyet negatif olamaz.");

        if (dto.UnitPrice < 0)
            errors.Add("Birim fiyat negatif olamaz.");

        if (dto.MaxStockLevel < dto.MinStockLevel)
            errors.Add("Maksimum stok seviyesi minimumdan küçük olamaz.");

        if (dto.ProductCategoryId == Guid.Empty)
            errors.Add("Kategori seçimi zorunludur.");

        return errors;
    }

    // ── Mapper ───────────────────────────────────────────────────────────────
    private static ProductDto MapToDto(Product entity, int totalStock) => new()
    {
        Id = entity.Id,
        CompanyId = entity.CompanyId,
        SKU = entity.SKU,
        Name = entity.Name,
        Description = entity.Description,
        Unit = entity.Unit,
        Barcode = entity.Barcode,
        UnitCost = entity.UnitCost,
        UnitPrice = entity.UnitPrice,
        MinStockLevel = entity.MinStockLevel,
        MaxStockLevel = entity.MaxStockLevel,
        IsActive = entity.IsActive,
        ProductCategoryId = entity.ProductCategoryId,
        CategoryName = entity.Category?.Name ?? string.Empty,
        CategoryColorCode = entity.Category?.ColorCode ?? "#607D8B",
        TotalStock = totalStock,
        IsLowStock = totalStock <= entity.MinStockLevel,
        CreatedAt = entity.CreatedAt
    };
}
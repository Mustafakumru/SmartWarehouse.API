// Controllers/ProductController.cs
using Microsoft.AspNetCore.Mvc;
using SmartWarehouse.API.DTOs.Common;
using SmartWarehouse.API.DTOs.Product;
using SmartWarehouse.API.DTOs.ProductCategory;
using SmartWarehouse.API.Managers.Interfaces;

namespace SmartWarehouse.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    private readonly IProductManager _manager;

    public ProductController(IProductManager manager)
    {
        _manager = manager;
    }

    // ── GET ──────────────────────────────────────────────────────────────────

    /// <summary>
    /// Server-side pagination + arama + filtreleme.
    /// KIRMIZI ÇİZGİ: Client-side pagination yasak, tüm filtreler DB'de.
    /// GET /api/product/paged?companyId=ABC&pageNumber=1&pageSize=10&searchTerm=laptop
    /// </summary>
    [HttpGet("paged")]
    public async Task<IActionResult> GetPaged(
        [FromQuery] string companyId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] Guid? categoryId = null,
        [FromQuery] bool? isActive = null)
    {
        if (string.IsNullOrWhiteSpace(companyId))
            return BadRequest("CompanyId zorunludur.");

        var filter = new ProductFilterDto
        {
            CompanyId = companyId,
            PageNumber = pageNumber,
            PageSize = pageSize,
            SearchTerm = searchTerm,
            CategoryId = categoryId,
            IsActive = isActive
        };

        var result = await _manager.GetPagedAsync(filter);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Tekil ürün getirir.
    /// GET /api/product/{id}?companyId=ABC
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(
        [FromRoute] Guid id,
        [FromQuery] string companyId)
    {
        if (string.IsNullOrWhiteSpace(companyId))
            return BadRequest("CompanyId zorunludur.");

        var result = await _manager.GetByIdAsync(id, companyId);

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    /// <summary>
    /// Aktif ürün listesi (dropdown gibi seçim alanları için).
    /// GET /api/product/active?companyId=ABC
    /// </summary>
    [HttpGet("active")]
    public async Task<IActionResult> GetActiveProducts([FromQuery] string companyId)
    {
        if (string.IsNullOrWhiteSpace(companyId))
            return BadRequest("CompanyId zorunludur.");

        var result = await _manager.GetActiveProductsAsync(companyId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    // ── POST ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Yeni ürün tanımı oluşturur.
    /// POST /api/product/create
    /// </summary>
    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] CreateProductDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.CompanyId))
            return BadRequest("CompanyId zorunludur.");

        var result = await _manager.CreateAsync(dto);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Ürün günceller. PUT değil POST.
    /// POST /api/product/update
    /// </summary>
    [HttpPost("update")]
    public async Task<IActionResult> Update([FromBody] UpdateProductDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.CompanyId))
            return BadRequest("CompanyId zorunludur.");

        if (dto.Id == Guid.Empty)
            return BadRequest("Geçerli bir Id zorunludur.");

        // KIRMIZI ÇİZGİ: Body'deki CompanyId ile Id eşleşmesi
        // Manager katmanında da kontrol ediliyor, çift güvenlik.
        var result = await _manager.UpdateAsync(dto);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Ürün siler (soft delete). DELETE değil POST.
    /// POST /api/product/delete
    /// Body: { "id": "...", "companyId": "..." }
    /// </summary>
    [HttpPost("delete")]
    public async Task<IActionResult> Delete([FromBody] DeleteRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.CompanyId))
            return BadRequest("CompanyId zorunludur.");

        if (dto.Id == Guid.Empty)
            return BadRequest("Geçerli bir Id zorunludur.");

        var result = await _manager.DeleteAsync(dto.Id, dto.CompanyId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}
// Controllers/ProductCategoryController.cs
using Microsoft.AspNetCore.Mvc;
using SmartWarehouse.API.DTOs.ProductCategory;
using SmartWarehouse.API.Managers.Interfaces;

namespace SmartWarehouse.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductCategoryController : ControllerBase
{
    private readonly IProductCategoryManager _manager;

    public ProductCategoryController(IProductCategoryManager manager)
    {
        _manager = manager;
    }

    // ── GET ──────────────────────────────────────────────────────────────────

    /// <summary>
    /// Şirkete ait tüm kategorileri listeler.
    /// </summary>
    [HttpGet("list")]
    public async Task<IActionResult> GetAll([FromQuery] string companyId)
    {
        // KIRMIZI ÇİZGİ: CompanyId zorunlu kontrolü
        if (string.IsNullOrWhiteSpace(companyId))
            return BadRequest("CompanyId zorunludur.");

        var result = await _manager.GetAllAsync(companyId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Tekil kategori getirir.
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

    // ── POST ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Yeni kategori oluşturur.
    /// </summary>
    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] CreateProductCategoryDto dto)
    {
        // KIRMIZI ÇİZGİ: Body içinden gelen CompanyId kontrolü
        if (string.IsNullOrWhiteSpace(dto.CompanyId))
            return BadRequest("CompanyId zorunludur.");

        var result = await _manager.CreateAsync(dto);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Kategori günceller. PUT değil POST kullanılır.
    /// </summary>
    [HttpPost("update")]
    public async Task<IActionResult> Update([FromBody] UpdateProductCategoryDto dto)
    {
        // KIRMIZI ÇİZGİ: CompanyId + Id kontrolü
        if (string.IsNullOrWhiteSpace(dto.CompanyId))
            return BadRequest("CompanyId zorunludur.");

        if (dto.Id == Guid.Empty)
            return BadRequest("Geçerli bir Id zorunludur.");

        var result = await _manager.UpdateAsync(dto);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Kategori siler (soft delete). DELETE değil POST kullanılır.
    /// </summary>
    [HttpPost("delete")]
    public async Task<IActionResult> Delete([FromBody] DeleteRequestDto dto)
    {
        // KIRMIZI ÇİZGİ: CompanyId + Id kontrolü
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
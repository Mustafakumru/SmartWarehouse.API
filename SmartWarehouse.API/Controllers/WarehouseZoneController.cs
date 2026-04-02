// Controllers/WarehouseZoneController.cs
using Microsoft.AspNetCore.Mvc;
using SmartWarehouse.API.DTOs.Common;
using SmartWarehouse.API.DTOs.ProductCategory;
using SmartWarehouse.API.DTOs.WarehouseZone;
using SmartWarehouse.API.Managers.Interfaces;

namespace SmartWarehouse.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WarehouseZoneController : ControllerBase
{
    private readonly IWarehouseZoneManager _manager;

    public WarehouseZoneController(IWarehouseZoneManager manager)
    {
        _manager = manager;
    }

    // ── GET ──────────────────────────────────────────────────────────────────

    [HttpGet("list")]
    public async Task<IActionResult> GetAll([FromQuery] string companyId)
    {
        if (string.IsNullOrWhiteSpace(companyId))
            return BadRequest("CompanyId zorunludur.");

        var result = await _manager.GetAllAsync(companyId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

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

    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] CreateWarehouseZoneDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.CompanyId))
            return BadRequest("CompanyId zorunludur.");

        var result = await _manager.CreateAsync(dto);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Bölge günceller. PUT değil POST.
    /// </summary>
    [HttpPost("update")]
    public async Task<IActionResult> Update([FromBody] UpdateWarehouseZoneDto dto)
    {
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
    /// Bölge siler (soft delete). DELETE değil POST.
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
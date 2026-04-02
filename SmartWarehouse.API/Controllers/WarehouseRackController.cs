// Controllers/WarehouseRackController.cs
using Microsoft.AspNetCore.Mvc;
using SmartWarehouse.API.DTOs.Common;
using SmartWarehouse.API.DTOs.ProductCategory;
using SmartWarehouse.API.DTOs.WarehouseRack;
using SmartWarehouse.API.Managers.Interfaces;

namespace SmartWarehouse.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WarehouseRackController : ControllerBase
{
    private readonly IWarehouseRackManager _manager;

    public WarehouseRackController(IWarehouseRackManager manager)
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

    /// <summary>
    /// Belirli bir bölgedeki rafları getirir.
    /// </summary>
    [HttpGet("by-zone/{zoneId}")]
    public async Task<IActionResult> GetByZone(
        [FromRoute] Guid zoneId,
        [FromQuery] string companyId)
    {
        if (string.IsNullOrWhiteSpace(companyId))
            return BadRequest("CompanyId zorunludur.");

        if (zoneId == Guid.Empty)
            return BadRequest("Geçerli bir ZoneId zorunludur.");

        var result = await _manager.GetByZoneAsync(zoneId, companyId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    // ── POST ─────────────────────────────────────────────────────────────────

    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] CreateWarehouseRackDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.CompanyId))
            return BadRequest("CompanyId zorunludur.");

        var result = await _manager.CreateAsync(dto);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("update")]
    public async Task<IActionResult> Update([FromBody] UpdateWarehouseRackDto dto)
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
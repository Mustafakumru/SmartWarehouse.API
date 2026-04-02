// Controllers/InventoryTransactionController.cs
using Microsoft.AspNetCore.Mvc;
using SmartWarehouse.API.DTOs.InventoryTransaction;
using SmartWarehouse.API.Managers.Interfaces;
using SmartWarehouse.API.Entities;

namespace SmartWarehouse.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InventoryTransactionController : ControllerBase
{
    private readonly IInventoryTransactionManager _manager;

    public InventoryTransactionController(IInventoryTransactionManager manager)
    {
        _manager = manager;
    }

    // ── GET ──────────────────────────────────────────────────────────────────

    /// <summary>
    /// Stok hareketleri — server-side pagination + filtreleme.
    /// GET /api/inventorytransaction/paged?companyId=ABC&pageNumber=1&pageSize=10
    /// </summary>
    [HttpGet("paged")]
    public async Task<IActionResult> GetPaged(
        [FromQuery] string companyId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? productId = null,
        [FromQuery] TransactionType? transactionType = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        if (string.IsNullOrWhiteSpace(companyId))
            return BadRequest("CompanyId zorunludur.");

        // Tarih aralığı mantık kontrolü
        if (startDate.HasValue && endDate.HasValue && startDate > endDate)
            return BadRequest("Başlangıç tarihi bitiş tarihinden büyük olamaz.");

        var filter = new TransactionFilterDto
        {
            CompanyId = companyId,
            PageNumber = pageNumber,
            PageSize = pageSize,
            ProductId = productId,
            TransactionType = transactionType,
            StartDate = startDate,
            EndDate = endDate
        };

        var result = await _manager.GetPagedAsync(filter);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Son N adet stok hareketini getirir (dashboard için).
    /// GET /api/inventorytransaction/recent?companyId=ABC&count=10
    /// </summary>
    [HttpGet("recent")]
    public async Task<IActionResult> GetRecent(
        [FromQuery] string companyId,
        [FromQuery] int count = 10)
    {
        if (string.IsNullOrWhiteSpace(companyId))
            return BadRequest("CompanyId zorunludur.");

        if (count < 1 || count > 100)
            return BadRequest("Count 1 ile 100 arasında olmalıdır.");

        var result = await _manager.GetRecentTransactionsAsync(companyId, count);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Bir ürünün tüm raflardaki anlık stok durumunu getirir.
    /// GET /api/inventorytransaction/stock/{productId}?companyId=ABC
    /// </summary>
    [HttpGet("stock/{productId}")]
    public async Task<IActionResult> GetStockByProduct(
        [FromRoute] Guid productId,
        [FromQuery] string companyId)
    {
        if (string.IsNullOrWhiteSpace(companyId))
            return BadRequest("CompanyId zorunludur.");

        if (productId == Guid.Empty)
            return BadRequest("Geçerli bir ProductId zorunludur.");

        var result = await _manager.GetStockByProductAsync(productId, companyId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    // ── POST ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Depoya ürün girişi (IN) veya çıkışı (OUT) işlemi.
    /// POST /api/inventorytransaction/create
    /// Body: { CompanyId, ProductId, WarehouseRackId, TransactionType, Quantity, ... }
    /// </summary>
    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] CreateTransactionDto dto)
    {
        // KIRMIZI ÇİZGİ: CompanyId zorunlu
        if (string.IsNullOrWhiteSpace(dto.CompanyId))
            return BadRequest("CompanyId zorunludur.");

        if (dto.ProductId == Guid.Empty)
            return BadRequest("Ürün seçimi zorunludur.");

        if (dto.WarehouseRackId == Guid.Empty)
            return BadRequest("Raf seçimi zorunludur.");

        if (dto.Quantity <= 0)
            return BadRequest("Miktar 0'dan büyük olmalıdır.");

        var result = await _manager.CreateTransactionAsync(dto);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}
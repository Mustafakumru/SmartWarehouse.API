// Managers/InventoryTransactionManager.cs
using SmartWarehouse.API.DTOs.Common;
using SmartWarehouse.API.DTOs.InventoryTransaction;
using SmartWarehouse.API.DTOs.WarehouseStock;
using SmartWarehouse.API.Entities;
using SmartWarehouse.API.Managers.Interfaces;
using SmartWarehouse.API.Repositories.Interfaces;

namespace SmartWarehouse.API.Managers;

public class InventoryTransactionManager : IInventoryTransactionManager
{
    private readonly IInventoryTransactionRepository _transactionRepository;
    private readonly IWarehouseStockRepository _stockRepository;
    private readonly IProductRepository _productRepository;
    private readonly IWarehouseRackRepository _rackRepository;

    public InventoryTransactionManager(
        IInventoryTransactionRepository transactionRepository,
        IWarehouseStockRepository stockRepository,
        IProductRepository productRepository,
        IWarehouseRackRepository rackRepository)
    {
        _transactionRepository = transactionRepository;
        _stockRepository = stockRepository;
        _productRepository = productRepository;
        _rackRepository = rackRepository;
    }

    public async Task<ApiResponseDto<PagedResponseDto<InventoryTransactionDto>>> GetPagedAsync(
        TransactionFilterDto filter)
    {
        if (string.IsNullOrWhiteSpace(filter.CompanyId))
            return ApiResponseDto<PagedResponseDto<InventoryTransactionDto>>
                .Fail("CompanyId zorunludur.");

        if (filter.PageNumber < 1) filter.PageNumber = 1;
        if (filter.PageSize < 1 || filter.PageSize > 100) filter.PageSize = 10;

        var (items, totalCount) = await _transactionRepository.GetPagedTransactionsAsync(
            filter.CompanyId,
            filter.PageNumber,
            filter.PageSize,
            filter.ProductId,
            filter.TransactionType,
            filter.StartDate,
            filter.EndDate);

        var dtos = items.Select(MapToDto);

        var pagedResponse = new PagedResponseDto<InventoryTransactionDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            PageNumber = filter.PageNumber,
            PageSize = filter.PageSize
        };

        return ApiResponseDto<PagedResponseDto<InventoryTransactionDto>>.Ok(pagedResponse);
    }

    public async Task<ApiResponseDto<InventoryTransactionDto>> CreateTransactionAsync(
        CreateTransactionDto dto)
    {
        // ── Validasyon ────────────────────────────────────────────────────────
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(dto.CompanyId))
            errors.Add("CompanyId zorunludur.");

        if (dto.ProductId == Guid.Empty)
            errors.Add("Ürün seçimi zorunludur.");

        if (dto.WarehouseRackId == Guid.Empty)
            errors.Add("Raf seçimi zorunludur.");

        if (dto.Quantity <= 0)
            errors.Add("Miktar 0'dan büyük olmalıdır.");

        if (dto.UnitCost < 0)
            errors.Add("Birim maliyet negatif olamaz.");

        if (string.IsNullOrWhiteSpace(dto.CreatedBy))
            errors.Add("İşlemi yapan kullanıcı zorunludur.");

        if (errors.Any())
            return ApiResponseDto<InventoryTransactionDto>.Fail("Validasyon hatası.", errors);

        // ── Ürün ve Raf varlık + CompanyId kontrolü ───────────────────────────
        var product = await _productRepository.GetByIdAsync(dto.ProductId, dto.CompanyId);
        if (product is null)
            return ApiResponseDto<InventoryTransactionDto>
                .Fail("Ürün bulunamadı veya bu şirkete ait değil.");

        var rack = await _rackRepository.GetByIdAsync(dto.WarehouseRackId, dto.CompanyId);
        if (rack is null)
            return ApiResponseDto<InventoryTransactionDto>
                .Fail("Raf bulunamadı veya bu şirkete ait değil.");

        // ── Mevcut stok satırını bul veya oluştur ────────────────────────────
        var stock = await _stockRepository.GetByProductAndRackAsync(
            dto.ProductId, dto.WarehouseRackId, dto.CompanyId);

        // ── Çıkış işlemi için yeterli stok kontrolü ──────────────────────────
        if (dto.TransactionType == TransactionType.OUT)
        {
            var availableQty = stock?.Quantity ?? 0;
            if (availableQty < dto.Quantity)
                return ApiResponseDto<InventoryTransactionDto>
                    .Fail($"Yetersiz stok. Mevcut: {availableQty}, Talep edilen: {dto.Quantity}");
        }

        // ── Stok güncelle ─────────────────────────────────────────────────────
        int stockAfter;

        if (stock is null)
        {
            // İlk kez stok girişi — yeni satır oluştur
            stock = new WarehouseStock
            {
                CompanyId = dto.CompanyId,
                ProductId = dto.ProductId,
                WarehouseRackId = dto.WarehouseRackId,
                Quantity = dto.TransactionType == TransactionType.IN ? dto.Quantity : 0,
                LastMovementAt = DateTime.UtcNow
            };
            await _stockRepository.CreateAsync(stock);
            stockAfter = stock.Quantity;
        }
        else
        {
            // Mevcut stok satırını güncelle
            stock.Quantity = dto.TransactionType switch
            {
                TransactionType.IN => stock.Quantity + dto.Quantity,
                TransactionType.OUT => stock.Quantity - dto.Quantity,
                TransactionType.ADJ => dto.Quantity,         // Sayım düzeltmesi: direkt set
                TransactionType.TRF => stock.Quantity - dto.Quantity, // Kaynak raftan düş
                _ => stock.Quantity
            };
            stock.LastMovementAt = DateTime.UtcNow;

            // KIRMIZI ÇİZGİ: EntityState.Modified — BaseRepository.UpdateAsync içinde
            await _stockRepository.UpdateAsync(stock);
            stockAfter = stock.Quantity;
        }

        // ── İşlem kodu üret ───────────────────────────────────────────────────
        var transactionCode = await _transactionRepository
            .GenerateTransactionCodeAsync(dto.CompanyId);

        // ── Transaction kaydı oluştur ─────────────────────────────────────────
        var transaction = new InventoryTransaction
        {
            CompanyId = dto.CompanyId,
            TransactionCode = transactionCode,
            ProductId = dto.ProductId,
            WarehouseRackId = dto.WarehouseRackId,
            TransactionType = dto.TransactionType,
            Quantity = dto.Quantity,
            UnitCost = dto.UnitCost,
            ReferenceNumber = dto.ReferenceNumber,
            Notes = dto.Notes,
            CreatedBy = dto.CreatedBy,
            TransactionDate = DateTime.UtcNow,
            StockAfterTransaction = stockAfter
        };

        var created = await _transactionRepository.CreateAsync(transaction);

        // Navigation property'leri doldur
        created.Product = product;
        created.WarehouseRack = rack;

        return ApiResponseDto<InventoryTransactionDto>
            .Ok(MapToDto(created), "Stok hareketi başarıyla kaydedildi.");
    }

    public async Task<ApiResponseDto<IEnumerable<WarehouseStockDto>>> GetStockByProductAsync(
        Guid productId, string companyId)
    {
        if (string.IsNullOrWhiteSpace(companyId))
            return ApiResponseDto<IEnumerable<WarehouseStockDto>>.Fail("CompanyId zorunludur.");

        var stocks = await _stockRepository.GetStockByProductAsync(productId, companyId);
        var dtos = stocks.Select(s => new WarehouseStockDto
        {
            Id = s.Id,
            ProductId = s.ProductId,
            ProductName = s.Product?.Name ?? string.Empty,
            ProductSKU = s.Product?.SKU ?? string.Empty,
            WarehouseRackId = s.WarehouseRackId,
            RackCode = s.WarehouseRack?.RackCode ?? string.Empty,
            ZoneName = s.WarehouseRack?.WarehouseZone?.ZoneName ?? string.Empty,
            Quantity = s.Quantity,
            ReservedQuantity = s.ReservedQuantity,
            AvailableQuantity = s.Quantity - s.ReservedQuantity,
            LastMovementAt = s.LastMovementAt
        });

        return ApiResponseDto<IEnumerable<WarehouseStockDto>>.Ok(dtos);
    }

    public async Task<ApiResponseDto<IEnumerable<InventoryTransactionDto>>>
        GetRecentTransactionsAsync(string companyId, int count = 10)
    {
        if (string.IsNullOrWhiteSpace(companyId))
            return ApiResponseDto<IEnumerable<InventoryTransactionDto>>
                .Fail("CompanyId zorunludur.");

        var transactions = await _transactionRepository
            .GetRecentTransactionsAsync(companyId, count);

        return ApiResponseDto<IEnumerable<InventoryTransactionDto>>
            .Ok(transactions.Select(MapToDto));
    }

    // ── Mapper ───────────────────────────────────────────────────────────────
    private static InventoryTransactionDto MapToDto(InventoryTransaction t) => new()
    {
        Id = t.Id,
        CompanyId = t.CompanyId,
        TransactionCode = t.TransactionCode,
        ProductId = t.ProductId,
        ProductName = t.Product?.Name ?? string.Empty,
        ProductSKU = t.Product?.SKU ?? string.Empty,
        WarehouseRackId = t.WarehouseRackId,
        RackCode = t.WarehouseRack?.RackCode ?? string.Empty,
        ZoneName = t.WarehouseRack?.WarehouseZone?.ZoneName ?? string.Empty,
        TransactionType = t.TransactionType,
        TransactionTypeName = t.TransactionType switch
        {
            TransactionType.IN => "Giriş",
            TransactionType.OUT => "Çıkış",
            TransactionType.ADJ => "Düzeltme",
            TransactionType.TRF => "Transfer",
            _ => "Bilinmiyor"
        },
        Quantity = t.Quantity,
        UnitCost = t.UnitCost,
        TotalCost = t.Quantity * t.UnitCost,
        ReferenceNumber = t.ReferenceNumber,
        Notes = t.Notes,
        CreatedBy = t.CreatedBy,
        StockAfterTransaction = t.StockAfterTransaction,
        TransactionDate = t.TransactionDate
    };
}
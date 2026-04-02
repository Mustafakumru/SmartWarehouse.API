// Repositories/InventoryTransactionRepository.cs
using Microsoft.EntityFrameworkCore;
using SmartWarehouse.API.Data;
using SmartWarehouse.API.Entities;
using SmartWarehouse.API.Repositories.Interfaces;

namespace SmartWarehouse.API.Repositories;

public class InventoryTransactionRepository
    : BaseRepository<InventoryTransaction>, IInventoryTransactionRepository
{
    public InventoryTransactionRepository(SmartWarehouseDbContext context)
        : base(context) { }

    public async Task<(IEnumerable<InventoryTransaction> Items, int TotalCount)>
        GetPagedTransactionsAsync(
            string companyId,
            int pageNumber,
            int pageSize,
            Guid? productId,
            TransactionType? transactionType,
            DateTime? startDate,
            DateTime? endDate)
    {
        var query = _dbSet
            .Include(t => t.Product)
            .Include(t => t.WarehouseRack)
                .ThenInclude(r => r.WarehouseZone)
            .Where(t => t.CompanyId == companyId);

        if (productId.HasValue)
            query = query.Where(t => t.ProductId == productId.Value);

        if (transactionType.HasValue)
            query = query.Where(t => t.TransactionType == transactionType.Value);

        if (startDate.HasValue)
            query = query.Where(t => t.TransactionDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(t => t.TransactionDate <= endDate.Value);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(t => t.TransactionDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<string> GenerateTransactionCodeAsync(string companyId)
    {
        // Format: TXN-20240115-0001
        var today = DateTime.UtcNow.ToString("yyyyMMdd");
        var prefix = $"TXN-{today}-";

        // Bugün bu şirket için kaç işlem yapılmış?
        var todayStart = DateTime.UtcNow.Date;
        var todayEnd = todayStart.AddDays(1);

        var countToday = await _dbSet
            .CountAsync(t =>
                t.CompanyId == companyId &&
                t.TransactionDate >= todayStart &&
                t.TransactionDate < todayEnd);

        // Sıradaki numarayı 4 haneli olarak formatla
        var sequence = (countToday + 1).ToString("D4");
        return $"{prefix}{sequence}";
    }

    public async Task<IEnumerable<InventoryTransaction>> GetRecentTransactionsAsync(
        string companyId, int count = 10)
    {
        return await _dbSet
            .Include(t => t.Product)
            .Include(t => t.WarehouseRack)
            .Where(t => t.CompanyId == companyId)
            .OrderByDescending(t => t.TransactionDate)
            .Take(count)
            .ToListAsync();
    }
}
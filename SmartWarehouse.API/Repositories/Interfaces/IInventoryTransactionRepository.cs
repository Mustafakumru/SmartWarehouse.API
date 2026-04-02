// Repositories/Interfaces/IInventoryTransactionRepository.cs
using SmartWarehouse.API.Entities;

namespace SmartWarehouse.API.Repositories.Interfaces;

public interface IInventoryTransactionRepository : IBaseRepository<InventoryTransaction>
{
    /// <summary>
    /// Server-side pagination + arama + filtreleme — stok hareketleri için
    /// </summary>
    Task<(IEnumerable<InventoryTransaction> Items, int TotalCount)> GetPagedTransactionsAsync(
        string companyId,
        int pageNumber,
        int pageSize,
        Guid? productId,
        TransactionType? transactionType,
        DateTime? startDate,
        DateTime? endDate);

    Task<string> GenerateTransactionCodeAsync(string companyId);
    Task<IEnumerable<InventoryTransaction>> GetRecentTransactionsAsync(
        string companyId,
        int count = 10);
}
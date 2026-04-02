// Repositories/Interfaces/IWarehouseStockRepository.cs
using SmartWarehouse.API.Entities;

namespace SmartWarehouse.API.Repositories.Interfaces;

public interface IWarehouseStockRepository : IBaseRepository<WarehouseStock>
{
    /// <summary>
    /// Belirli ürünün belirli raftaki stok kaydını getirir.
    /// Giriş/Çıkış işlemlerinde mevcut stok satırını bulmak için kullanılır.
    /// </summary>
    Task<WarehouseStock?> GetByProductAndRackAsync(
        Guid productId,
        Guid rackId,
        string companyId);

    /// <summary>
    /// Bir ürünün tüm raflardaki toplam stok miktarını döndürür.
    /// </summary>
    Task<int> GetTotalStockByProductAsync(Guid productId, string companyId);

    Task<IEnumerable<WarehouseStock>> GetStockByProductAsync(Guid productId, string companyId);
    Task<IEnumerable<WarehouseStock>> GetStockByRackAsync(Guid rackId, string companyId);

    /// <summary>
    /// Kritik stok seviyesinin altına düşmüş ürünleri getirir.
    /// </summary>
    Task<IEnumerable<WarehouseStock>> GetCriticalStocksAsync(string companyId);
}
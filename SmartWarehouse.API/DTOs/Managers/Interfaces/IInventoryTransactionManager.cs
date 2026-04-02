// Managers/Interfaces/IInventoryTransactionManager.cs
using SmartWarehouse.API.DTOs.Common;
using SmartWarehouse.API.DTOs.InventoryTransaction;
using SmartWarehouse.API.DTOs.WarehouseStock;

namespace SmartWarehouse.API.Managers.Interfaces;

public interface IInventoryTransactionManager
{
    Task<ApiResponseDto<PagedResponseDto<InventoryTransactionDto>>> GetPagedAsync(
        TransactionFilterDto filter);

    Task<ApiResponseDto<InventoryTransactionDto>> CreateTransactionAsync(
        CreateTransactionDto dto);

    Task<ApiResponseDto<IEnumerable<WarehouseStockDto>>> GetStockByProductAsync(
        Guid productId, string companyId);

    Task<ApiResponseDto<IEnumerable<InventoryTransactionDto>>> GetRecentTransactionsAsync(
        string companyId, int count = 10);
}
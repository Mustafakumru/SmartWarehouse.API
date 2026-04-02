// Managers/Interfaces/IProductManager.cs
using SmartWarehouse.API.DTOs.Common;
using SmartWarehouse.API.DTOs.Product;

namespace SmartWarehouse.API.Managers.Interfaces;

public interface IProductManager
{
    Task<ApiResponseDto<PagedResponseDto<ProductListDto>>> GetPagedAsync(ProductFilterDto filter);
    Task<ApiResponseDto<ProductDto>> GetByIdAsync(Guid id, string companyId);
    Task<ApiResponseDto<ProductDto>> CreateAsync(CreateProductDto dto);
    Task<ApiResponseDto<ProductDto>> UpdateAsync(UpdateProductDto dto);
    Task<ApiResponseDto<bool>> DeleteAsync(Guid id, string companyId);
    Task<ApiResponseDto<IEnumerable<ProductListDto>>> GetActiveProductsAsync(string companyId);
}
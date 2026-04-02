// Managers/Interfaces/IProductCategoryManager.cs
using SmartWarehouse.API.DTOs.Common;
using SmartWarehouse.API.DTOs.ProductCategory;

namespace SmartWarehouse.API.Managers.Interfaces;

public interface IProductCategoryManager
{
    Task<ApiResponseDto<IEnumerable<ProductCategoryDto>>> GetAllAsync(string companyId);
    Task<ApiResponseDto<ProductCategoryDto>> GetByIdAsync(Guid id, string companyId);
    Task<ApiResponseDto<ProductCategoryDto>> CreateAsync(CreateProductCategoryDto dto);
    Task<ApiResponseDto<ProductCategoryDto>> UpdateAsync(UpdateProductCategoryDto dto);
    Task<ApiResponseDto<bool>> DeleteAsync(Guid id, string companyId);
}
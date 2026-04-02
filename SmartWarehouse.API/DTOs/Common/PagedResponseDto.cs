// DTOs/Common/PagedResponseDto.cs
namespace SmartWarehouse.API.DTOs.Common;

/// <summary>
/// Server-side pagination için standart response zarfı.
/// Tüm sayfalı listeler bu tip üzerinden döner.
/// </summary>
public class PagedResponseDto<T>
{
    public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}
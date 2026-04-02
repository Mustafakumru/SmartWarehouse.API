// DTOs/Common/ApiResponseDto.cs
namespace SmartWarehouse.API.DTOs.Common;

/// <summary>
/// Tüm API yanıtları için standart zarf.
/// Frontend her zaman bu yapıyı bekler.
/// </summary>
public class ApiResponseDto<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public IEnumerable<string> Errors { get; set; } = Enumerable.Empty<string>();

    public static ApiResponseDto<T> Ok(T data, string message = "İşlem başarılı.")
        => new() { Success = true, Message = message, Data = data };

    public static ApiResponseDto<T> Fail(string message, IEnumerable<string>? errors = null)
        => new() { Success = false, Message = message, Errors = errors ?? Enumerable.Empty<string>() };
}
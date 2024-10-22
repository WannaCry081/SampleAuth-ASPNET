using Newtonsoft.Json;
using static sample_auth_aspnet.Models.Utils.Errors;

namespace sample_auth_aspnet.Models.Response;

public class ApiResponse<T>
{
    public bool Success { get; init; }
    public T? Data { get; init; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string? Message { get; init; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public ErrorType? ErrorType { get; init; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public List<string>? ValidationErors { get; init; }

    public static ApiResponse<T> SuccessResponse(T data)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data,
        };
    }

    public static ApiResponse<T> ErrorResponse(string message, ErrorType errorType, List<string>? validationErrors = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            ErrorType = errorType,
            ValidationErors = validationErrors
        };
    }
}

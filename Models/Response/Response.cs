using Newtonsoft.Json;
using static sample_auth_aspnet.Models.Utils.Error;

namespace sample_auth_aspnet.Models.Response;

public class ApiResponse<T>
{
    public bool Success { get; init; }
    public string Message { get; init; } = null!;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public T? Data { get; init; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public ErrorType? Title { get; init; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public Dictionary<string, string>? Details { get; init; }

    public static ApiResponse<T> SuccessResponse(T? data, string message = "")
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data,
            Message = message
        };
    }

    public static ApiResponse<T> ErrorResponse(
        string message, ErrorType errorType, Dictionary<string, string>? details = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Title = errorType,
            Details = details
        };
    }
}

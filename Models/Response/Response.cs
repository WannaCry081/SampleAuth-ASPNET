using Newtonsoft.Json;
using static sample_auth_aspnet.Models.Utils.Error;

namespace sample_auth_aspnet.Models.Response;

public class ApiResponse<T>
{
    public string Status { get; init; } = null!;
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
            Status = "success",
            Data = data,
            Message = message
        };
    }

    public static ApiResponse<T> ErrorResponse(string message, ErrorType errorType, Dictionary<string, string>? details = null)
    {
        return new ApiResponse<T>
        {
            Status = "error",
            Message = message,
            Title = errorType,
            Details = details
        };
    }
}

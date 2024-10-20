using Newtonsoft.Json;
using static sample_auth_aspnet.Models.Utils.Errors;

namespace sample_auth_aspnet.Models.Response;

public class ApiReponse<T>
{
    public bool Success { get; init; }
    public T? Data { get; init; }
    public string Message { get; init; } = string.Empty;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public ErrorType? ErrorType { get; init; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public List<string>? ValidationErors { get; init; }

    public static ApiReponse<T> SuccessResponse(T data, string message)
    {
        return new ApiReponse<T>
        {
            Success = true,
            Data = data,
            Message = message
        };
    }

    public static ApiReponse<T> ErrorResponse(T data, string message, List<string>? validationErrors = null)
    {
        return new ApiReponse<T>
        {
            Success = false,
            Data = data,
            Message = message,
            ValidationErors = validationErrors
        };
    }
}

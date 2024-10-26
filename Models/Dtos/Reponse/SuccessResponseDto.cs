namespace sample_auth_aspnet.Models.Dtos.Reponse;

/// <summary>
/// DTO used for wrapping successful response data.
/// </summary>
/// <typeparam name="T">The type of data included in the success response.</typeparam>
public class SuccessResponseDto<T>
{
    /// <summary>
    /// Indicates the status of the response, typically set to "success".
    /// </summary>
    public string Status { get; init; } = "success";

    /// <summary>
    /// Message providing details about the success of the operation.
    /// </summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// The data payload associated with the successful response.
    /// </summary>
    public T? Data { get; init; }
}

namespace sample_auth_aspnet.Models.Dtos.Reponse;

/// <summary>
///     DTO used for wrapping successful response data.
/// </summary>
/// <typeparam name="T">The type of data included in the success response.</typeparam>
public class SuccessResponseDto<T>
{
    /// <summary>
    ///     Indicates the success of the response.
    /// </summary>
    public bool Success { get; init; } = true;


    /// <summary>
    ///     Message providing details about the success of the operation.
    /// </summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    ///     The data payload associated with the successful response.
    /// </summary>
    public T? Data { get; init; }
}

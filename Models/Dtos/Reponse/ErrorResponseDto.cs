using static sample_auth_aspnet.Models.Utils.Error;

namespace sample_auth_aspnet.Models.Dtos.Reponse;

/// <summary>
///     DTO used for wrapping error response.
/// </summary>
public class ErrorResponseDto
{
    /// <summary>
    ///     Indicates the success of the response.
    /// </summary>
    public bool Success { get; init; } = false;

    /// <summary>
    ///     Message providing details about the error.
    /// </summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    ///     Type of error represented by the <see cref="ErrorType"/> enumeration.
    /// </summary>
    public ErrorType? Title { get; init; }

    /// <summary>
    ///     Additional error details, such as validation errors, as key-value pairs.
    /// </summary>
    public Dictionary<string, string>? Details { get; init; }
}

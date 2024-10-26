using static sample_auth_aspnet.Models.Utils.Error;

namespace sample_auth_aspnet.Models.Dtos.Reponse;

public class ErrorResponseDto
{
    public string Status { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public ErrorType? ErrorType { get; init; }
    public Dictionary<string, string>? Details { get; init; }
}

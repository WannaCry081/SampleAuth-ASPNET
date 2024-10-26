
namespace sample_auth_aspnet.Models.Dtos.Reponse;

public class SuccessResponseDto<T>
{
    public string Status { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public T? Data { get; init; }
}

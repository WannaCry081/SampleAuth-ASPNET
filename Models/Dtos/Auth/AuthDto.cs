namespace sample_auth_aspnet.Models.Dtos.Auth;

public class AuthDto
{
    public string Access { get; init; } = null!;
    public string Refresh { get; init; } = null!;
}

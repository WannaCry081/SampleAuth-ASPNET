namespace sample_auth_aspnet.Services.Utils;

public class JWTSettings
{
    public string Secret { get; set; } = string.Empty;
    public int RefreshTokenExpiry { get; set; }
    public int AccessTokenExpiry { get; set; }
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
}

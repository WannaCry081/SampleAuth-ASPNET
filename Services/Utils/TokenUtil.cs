using System.Text;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using sample_auth_aspnet.Models.Entities;
using System.IdentityModel.Tokens.Jwt;
using sample_auth_aspnet.Models.Dtos.Auth;

namespace sample_auth_aspnet.Services.Utils;

public static class TokenUtil
{
    private static string GenerateToken(User user, DateTime expires, IConfiguration configuration, bool isAccessToken = true)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, new DateTimeOffset(
                DateTime.UtcNow).ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64),
        };

        if (isAccessToken)
        {
            claims.Add(new(ClaimTypes.Email, user.Email));
            claims.Add(new(ClaimTypes.NameIdentifier, user.Id.ToString()));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: configuration["JWT:Issuer"],
            audience: configuration["JWT:Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public static string GenerateAccess(User user, IConfiguration configuration)
    {
        var expiry = DateTime.UtcNow.AddHours(
            Convert.ToDouble(configuration["JWT:AccessTokenExpiry"]));

        return GenerateToken(user, expiry, configuration);
    }

    public static string GenerateRefresh(User user, IConfiguration configuration)
    {
        var expiry = DateTime.UtcNow.AddDays(
            Convert.ToDouble(configuration["JWT:RefreshTokenExpiry"]));

        return GenerateToken(user, expiry, configuration, isAccessToken: false);
    }

    public static AuthDto GenerateTokens(User user, IConfiguration configuration)
    {
        return new AuthDto
        {
            Access = GenerateAccess(user, configuration),
            Refresh = GenerateRefresh(user, configuration)
        };
    }

    public static ClaimsPrincipal? ValidateRefreshToken(string refreshToken, IConfiguration configuration)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(configuration["JWT:Key"]!);

        try
        {
            var principal = tokenHandler.ValidateToken(refreshToken, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = configuration["JWT:Issuer"],
                ValidateAudience = true,
                ValidAudience = configuration["JWT:Audience"],
                ValidateLifetime = true,
            }, out SecurityToken validatedToken);

            return principal;
        }
        catch (Exception)
        {
            return null;
        }
    }
}

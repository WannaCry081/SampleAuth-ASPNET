using System.Text;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using sample_auth_aspnet.Models.Entities;
using sample_auth_aspnet.Models.Dtos.Auth;
using sample_auth_aspnet.Models.Utils;

namespace sample_auth_aspnet.Services.Utils;

public static class TokenUtil
{
    public enum TokenType
    {
        REFRESH,
        ACCESS,
        RESET
    }

    public static string GenerateToken(User user, JWTSettings jwt, TokenType type)
    {
        DateTime expires = DateTime.UtcNow;

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat,
                new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64)
        };

        switch (type)
        {
            case TokenType.REFRESH:
                expires = expires.AddDays(jwt.RefreshTokenExpiry);
                break;

            case TokenType.ACCESS:
                expires = expires.AddHours(jwt.AccessTokenExpiry);
                claims.Add(new(ClaimTypes.Email, user.Email));
                claims.Add(new(ClaimTypes.NameIdentifier, user.Id.ToString()));
                break;

            case TokenType.RESET:
                expires = expires.AddMinutes(jwt.ResetTokenExpiry);
                claims.Add(new(ClaimTypes.Email, user.Email));
                claims.Add(new("Purpose", "reset-password"));
                break;
        }

        claims.Add(new(JwtRegisteredClaimNames.Exp,
            new DateTimeOffset(expires).ToUnixTimeSeconds().ToString(),
            ClaimValueTypes.Integer64));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwt.Issuer,
            audience: jwt.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public static AuthDto GenerateTokens(User user, JWTSettings jwt)
    {
        return new AuthDto
        {
            Access = GenerateAccess(user, jwt),
            Refresh = GenerateRefresh(user, jwt)
        };
    }

    public static ClaimsPrincipal? ValidateRefreshToken(string refreshToken, JWTSettings jwt)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(jwt.Secret);

        try
        {
            var principal = tokenHandler.ValidateToken(refreshToken, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = jwt.Issuer,
                ValidateAudience = true,
                ValidAudience = jwt.Audience,
                ValidateLifetime = true,
            }, out SecurityToken validatedToken);

            return principal;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public static bool IsTokenNearExpiration(ClaimsPrincipal principal, int bufferMinutes)
    {
        var expClaim = principal.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Exp)?.Value;
        if (string.IsNullOrEmpty(expClaim) || !long.TryParse(expClaim, out var expSeconds))
        {
            return true;
        }

        var expirationTime = DateTimeOffset.FromUnixTimeSeconds(expSeconds).UtcDateTime;
        return expirationTime < DateTime.UtcNow.AddMinutes(bufferMinutes);
    }

}

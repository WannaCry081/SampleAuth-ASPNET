using System.Text;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using sample_auth_aspnet.Models.Entities;
using System.IdentityModel.Tokens.Jwt;

namespace sample_auth_aspnet.Services.Utils;

public static class TokenUtil
{
    private static string GenerateToken(List<Claim> claims, DateTime expires, IConfiguration configuration)
    {
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
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, new DateTimeOffset(
                DateTime.UtcNow).ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.NameIdentifier, user.Id.ToString())
        };

        var expiry = DateTime.UtcNow.AddHours(
            Convert.ToDouble(configuration["JWT:AccessTokenExpiry"]));

        return GenerateToken(claims, expiry, configuration);
    }

    public static string GenerateRefresh(User user, IConfiguration configuration, out string jtiValue)
    {
        jtiValue = Guid.NewGuid().ToString();
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, jtiValue),
            new(JwtRegisteredClaimNames.Iat, new DateTimeOffset(
                DateTime.UtcNow).ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64),
        };

        var expiry = DateTime.UtcNow.AddDays(
            Convert.ToDouble(configuration["JWT:RefreshTokenExpiry"]));

        return GenerateToken(claims, expiry, configuration);
    }

    public static ClaimsPrincipal? ValidateRefreshToken(string refreshToken, IConfiguration configuration, out SecurityToken? validatedToken)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = configuration["JWT:Issuer"],
            ValidateAudience = true,
            ValidAudience = configuration["JWT:Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(configuration["JWT:Key"]!)),
        };

        return tokenHandler.ValidateToken(
            refreshToken, validationParameters, out validatedToken);
    }
}


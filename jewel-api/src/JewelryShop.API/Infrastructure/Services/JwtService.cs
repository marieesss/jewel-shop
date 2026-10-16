using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using JewelryShop.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace JewelryShop.Infrastructure.Services;

public class JwtService : IJwtService
{
    public const int DefaultAccessTokenMinutes = 15;

    private readonly string _secret;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _accessTokenMinutes;

    public JwtService(IConfiguration configuration)
    {
        _secret = configuration["Jwt:Secret"] ?? throw new InvalidOperationException("Jwt:Secret manquant.");
        _issuer = configuration["Jwt:Issuer"] ?? "JewelryShop";
        _audience = configuration["Jwt:Audience"] ?? "JewelryShop";
        _accessTokenMinutes = int.TryParse(configuration["Jwt:AccessTokenMinutes"], out var minutes)
            ? minutes
            : DefaultAccessTokenMinutes;
    }

    public string GenerateToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub,   user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Role,               user.Role.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_accessTokenMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

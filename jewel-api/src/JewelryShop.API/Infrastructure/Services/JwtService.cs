using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using JewelryShop.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace JewelryShop.Infrastructure.Services;

public class JwtService : IJwtService
{
    private readonly string _secret;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _expiresInDays;

    public JwtService(IConfiguration configuration)
    {
        _secret   = configuration["Jwt:Secret"]   ?? throw new InvalidOperationException("Jwt:Secret manquant.");
        _issuer   = configuration["Jwt:Issuer"]   ?? "JewelryShop";
        _audience = configuration["Jwt:Audience"] ?? "JewelryShop";
        _expiresInDays = int.TryParse(configuration["Jwt:ExpiresInDays"], out var d) ? d : 7;
    }

    public string GenerateToken(User user)
    {
        var key         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub,   user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Role,               user.Role.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer:             _issuer,
            audience:           _audience,
            claims:             claims,
            expires:            DateTime.UtcNow.AddDays(_expiresInDays),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

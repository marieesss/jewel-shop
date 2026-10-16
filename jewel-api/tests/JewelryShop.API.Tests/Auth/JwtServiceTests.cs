using System.IdentityModel.Tokens.Jwt;
using JewelryShop.Application.Features.Users;
using JewelryShop.Infrastructure.Services;
using Microsoft.Extensions.Configuration;

namespace JewelryShop.API.Tests.Auth;

public class JwtServiceTests
{
    private const string Secret = "une-cle-de-test-suffisamment-longue-pour-hs256";

    private static readonly User _user = new() { Id = 1, Email = "marie@mycharmly.eu" };

    private static JwtService CreateService(Dictionary<string, string?> settings)
    {
        settings["Jwt:Secret"] = Secret;
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
        return new JwtService(configuration);
    }

    private static TimeSpan LifetimeOf(string token)
    {
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        return jwt.ValidTo - DateTime.UtcNow;
    }

    private static void AssertLifetimeIsAbout(TimeSpan expected, TimeSpan actual)
        => Assert.InRange(actual, expected - TimeSpan.FromSeconds(5), expected + TimeSpan.FromSeconds(1));

    [Fact]
    public void Le_token_expire_dans_15_minutes_par_defaut()
    {
        var service = CreateService(new Dictionary<string, string?>());

        var token = service.GenerateToken(_user);

        AssertLifetimeIsAbout(TimeSpan.FromMinutes(15), LifetimeOf(token));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(15)]
    [InlineData(60)]
    public void Le_token_expire_selon_AccessTokenMinutes_configure(int minutes)
    {
        var service = CreateService(new Dictionary<string, string?>
        {
            ["Jwt:AccessTokenMinutes"] = minutes.ToString(),
        });

        var token = service.GenerateToken(_user);

        AssertLifetimeIsAbout(TimeSpan.FromMinutes(minutes), LifetimeOf(token));
    }

    [Fact]
    public void Ignore_l_ancienne_variable_ExpiresInDays()
    {
        var service = CreateService(new Dictionary<string, string?>
        {
            ["Jwt:ExpiresInDays"] = "7",
        });

        var token = service.GenerateToken(_user);

        AssertLifetimeIsAbout(TimeSpan.FromMinutes(15), LifetimeOf(token));
    }
}

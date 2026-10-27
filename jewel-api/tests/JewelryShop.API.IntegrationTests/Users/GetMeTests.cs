using System.Net;
using JewelryShop.API.IntegrationTests.Chains;
using JewelryShop.API.IntegrationTests.Infrastructure;
using JewelryShop.Common.Enums;

namespace JewelryShop.API.IntegrationTests.Users;

[Collection(ApiCollection.Name)]
public class GetMeTests
{
    private readonly JewelryShopApiFactory _api;

    public GetMeTests(JewelryShopApiFactory api) => _api = api;

    [Fact]
    public async Task Renvoie_une_adresse_nulle_pour_un_utilisateur_sans_adresse()
    {
        var client = await _api.CreateClientAsAsync(UserRole.User);

        var profile = await client.GetMyProfileAsync();

        Assert.True(profile.Id > 0);
        Assert.Equal("Marie", profile.Name);
        Assert.Null(profile.Phone);
        Assert.Null(profile.Address);
    }

    [Fact]
    public async Task Refuse_un_appel_anonyme_avec_401()
    {
        var response = await _api.CreateApiClient().GetAsync(UsersApi.MePath);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}

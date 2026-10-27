using System.Net;
using System.Net.Http.Json;
using JewelryShop.API.IntegrationTests.Chains;
using JewelryShop.API.IntegrationTests.Infrastructure;
using JewelryShop.Common.Enums;
using JewelryShop.Features.Users.DTOs;

namespace JewelryShop.API.IntegrationTests.Users;

[Collection(ApiCollection.Name)]
public class UpdateMyAddressTests
{
    private readonly JewelryShopApiFactory _api;

    public UpdateMyAddressTests(JewelryShopApiFactory api) => _api = api;

    private static object AddressBody(string postalCode = "75011") => new
    {
        Line1 = "12 rue des Lilas",
        Line2 = "Bâtiment B",
        PostalCode = postalCode,
        City = "Paris",
        Phone = "0612345678"
    };

    [Fact]
    public async Task Enregistre_une_adresse_valide_que_le_profil_renvoie_ensuite()
    {
        var client = await _api.CreateClientAsAsync(UserRole.User);

        var response = await client.PutAsJsonAsync(UsersApi.MyAddressPath, AddressBody());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var saved = await response.Content.ReadFromJsonAsync<ProfileDto>();
        Assert.Equal(new AddressDto("12 rue des Lilas", "Bâtiment B", "75011", "Paris"), saved?.Address);

        var profile = await client.GetMyProfileAsync();
        Assert.Equal("0612345678", profile.Phone);
        Assert.Equal(new AddressDto("12 rue des Lilas", "Bâtiment B", "75011", "Paris"), profile.Address);
    }

    [Fact]
    public async Task Refuse_un_code_postal_de_4_chiffres_avec_400()
    {
        var client = await _api.CreateClientAsAsync(UserRole.User);

        var response = await client.PutAsJsonAsync(UsersApi.MyAddressPath, AddressBody(postalCode: "7500"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ApiError>();
        Assert.NotNull(error?.Errors);
        Assert.Contains("Le code postal doit contenir 5 chiffres.", Assert.Contains("PostalCode", error.Errors));

        var profile = await client.GetMyProfileAsync();
        Assert.Null(profile.Address);
    }

    [Fact]
    public async Task Refuse_un_appel_anonyme_avec_401()
    {
        var response = await _api.CreateApiClient().PutAsJsonAsync(UsersApi.MyAddressPath, AddressBody());

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}

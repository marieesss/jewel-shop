using System.Net;
using System.Net.Http.Json;
using JewelryShop.API.IntegrationTests.Chains;
using JewelryShop.API.IntegrationTests.Infrastructure;
using JewelryShop.Common.Enums;
using JewelryShop.Features.Orders.DTOs;

namespace JewelryShop.API.IntegrationTests.Orders;

[Collection(ApiCollection.Name)]
public class GetQuoteTests
{
    private readonly JewelryShopApiFactory _api;

    public GetQuoteTests(JewelryShopApiFactory api) => _api = api;

    [Fact]
    public async Task Calcule_un_total_de_16_50_pour_une_chaine_de_45_cm_et_deux_breloques_a_5_euros()
    {
        var admin = await _api.CreateClientAsAsync(UserRole.Admin);
        var chainId = await admin.CreateQuoteChainAsync();
        var clover = await admin.CreateQuoteCharmAsync();
        var heart = await admin.CreateQuoteCharmAsync();
        var user = await _api.CreateClientAsAsync(UserRole.User);

        var response = await user.PostAsJsonAsync(OrdersApi.QuotePath, OrdersApi.QuoteBody(chainId, heart.Id, clover.Id));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var quote = await response.Content.ReadFromJsonAsync<QuoteDto>();
        Assert.NotNull(quote);
        Assert.Equal(16.50m, quote.Total);
        Assert.Equal(
            [
                new QuoteLineDto("Chain", chainId, quote.Lines[0].Name, OrdersApi.OfferedLengthCm, null, 6.50m),
                new QuoteLineDto("Charm", heart.Id, heart.Name, null, 1, 5m),
                new QuoteLineDto("Charm", clover.Id, clover.Name, null, 2, 5m),
            ],
            quote.Lines);
    }

    [Fact]
    public async Task Ne_touche_pas_au_stock()
    {
        var admin = await _api.CreateClientAsAsync(UserRole.Admin);
        var chainId = await admin.CreateQuoteChainAsync();
        var charm = await admin.CreateQuoteCharmAsync(stock: 3);
        var user = await _api.CreateClientAsAsync(UserRole.User);

        var response = await user.PostAsJsonAsync(OrdersApi.QuotePath, OrdersApi.QuoteBody(chainId, charm.Id, charm.Id));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(3, await user.GetCharmStockAsync(charm.Id));
    }

    [Fact]
    public async Task Refuse_9_breloques_avec_400()
    {
        var admin = await _api.CreateClientAsAsync(UserRole.Admin);
        var chainId = await admin.CreateQuoteChainAsync();
        var charm = await admin.CreateQuoteCharmAsync(stock: 20);
        var user = await _api.CreateClientAsAsync(UserRole.User);

        var response = await user.PostAsJsonAsync(OrdersApi.QuotePath, OrdersApi.QuoteBody(chainId, Enumerable.Repeat(charm.Id, 9).ToArray()));

        await AssertValidationErrorAsync(response, "CharmIds", "8 breloques maximum.");
    }

    [Fact]
    public async Task Refuse_un_collier_sans_breloque_avec_400()
    {
        var admin = await _api.CreateClientAsAsync(UserRole.Admin);
        var chainId = await admin.CreateQuoteChainAsync();
        var user = await _api.CreateClientAsAsync(UserRole.User);

        var response = await user.PostAsJsonAsync(OrdersApi.QuotePath, OrdersApi.QuoteBody(chainId));

        await AssertValidationErrorAsync(response, "CharmIds", "Ajoutez au moins une breloque.");
    }

    [Fact]
    public async Task Refuse_une_longueur_non_proposee_avec_400()
    {
        var admin = await _api.CreateClientAsAsync(UserRole.Admin);
        var chainId = await admin.CreateQuoteChainAsync();
        var charm = await admin.CreateQuoteCharmAsync();
        var user = await _api.CreateClientAsAsync(UserRole.User);

        var response = await user.PostAsJsonAsync(OrdersApi.QuotePath, OrdersApi.QuoteBody(chainId, 46, [charm.Id]));

        await AssertValidationErrorAsync(response, "LengthCm", "Longueur non proposée.");
    }

    [Fact]
    public async Task Refuse_une_chaine_inexistante_avec_404()
    {
        var admin = await _api.CreateClientAsAsync(UserRole.Admin);
        var charm = await admin.CreateQuoteCharmAsync();
        var user = await _api.CreateClientAsAsync(UserRole.User);

        var response = await user.PostAsJsonAsync(OrdersApi.QuotePath, OrdersApi.QuoteBody(long.MaxValue, charm.Id));

        await AssertErrorAsync(response, HttpStatusCode.NotFound, "Chaîne introuvable.");
    }

    [Fact]
    public async Task Refuse_une_breloque_inexistante_avec_404()
    {
        var admin = await _api.CreateClientAsAsync(UserRole.Admin);
        var chainId = await admin.CreateQuoteChainAsync();
        var user = await _api.CreateClientAsAsync(UserRole.User);

        var response = await user.PostAsJsonAsync(OrdersApi.QuotePath, OrdersApi.QuoteBody(chainId, long.MaxValue));

        await AssertErrorAsync(response, HttpStatusCode.NotFound, "Breloque introuvable.");
    }

    [Fact]
    public async Task Refuse_3_exemplaires_d_une_breloque_dont_le_stock_est_2_avec_409()
    {
        var admin = await _api.CreateClientAsAsync(UserRole.Admin);
        var chainId = await admin.CreateQuoteChainAsync();
        var charm = await admin.CreateQuoteCharmAsync(stock: 2);
        var user = await _api.CreateClientAsAsync(UserRole.User);

        var response = await user.PostAsJsonAsync(OrdersApi.QuotePath, OrdersApi.QuoteBody(chainId, charm.Id, charm.Id, charm.Id));

        await AssertErrorAsync(response, HttpStatusCode.Conflict, $"Le stock de « {charm.Name} » ne permet plus cette commande.");
    }

    [Fact]
    public async Task Refuse_une_longueur_qu_aucun_rouleau_ne_couvre_avec_409()
    {
        var admin = await _api.CreateClientAsAsync(UserRole.Admin);
        var chainId = await admin.CreateQuoteChainAsync(rollLengthCm: 40);
        var charm = await admin.CreateQuoteCharmAsync();
        var user = await _api.CreateClientAsAsync(UserRole.User);

        var response = await user.PostAsJsonAsync(OrdersApi.QuotePath, OrdersApi.QuoteBody(chainId, charm.Id));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Refuse_un_appel_anonyme_avec_401()
    {
        var response = await _api.CreateApiClient().PostAsJsonAsync(OrdersApi.QuotePath, OrdersApi.QuoteBody(1, 1));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private static async Task AssertValidationErrorAsync(HttpResponseMessage response, string field, string message)
    {
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ApiError>();
        Assert.NotNull(error?.Errors);
        Assert.Contains(message, Assert.Contains(field, error.Errors));
    }

    private static async Task AssertErrorAsync(HttpResponseMessage response, HttpStatusCode status, string title)
    {
        Assert.Equal(status, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ApiError>();
        Assert.Equal(title, error?.Title);
    }
}

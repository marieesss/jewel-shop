using System.Net;
using System.Net.Http.Json;
using JewelryShop.API.IntegrationTests.Infrastructure;
using JewelryShop.Common.Enums;
using JewelryShop.Features.Chains.DTOs;
using JewelryShop.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace JewelryShop.API.IntegrationTests.Chains;

[Collection(ApiCollection.Name)]
public class CreateChainTests
{
    private readonly JewelryShopApiFactory _api;

    public CreateChainTests(JewelryShopApiFactory api) => _api = api;

    [Fact]
    public async Task Cree_une_chaine_a_l_ancien_format_avec_ses_prix_au_metre_et_un_rouleau()
    {
        var admin = await _api.CreateClientAsAsync(UserRole.Admin);

        var response = await admin.PostAsJsonAsync(ChainsApi.Path, ChainsApi.LegacyBody(ChainsApi.UniqueName()));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var chain = await response.Content.ReadFromJsonAsync<AdminChainDto>();
        Assert.NotNull(chain);
        Assert.Equal(20m, chain.Cost);
        Assert.Equal(50m, chain.Price);
        Assert.Equal(500m, chain.Length);
        Assert.Equal(4m, chain.CostPerMeter);
        Assert.Equal(10m, chain.PricePerMeter);
        Assert.Equal(0m, chain.ClaspCost);
        Assert.Equal(0m, chain.ClaspPrice);
        Assert.Equal(500, chain.StockCm);
        Assert.Equal([500], await RollLengthsAsync(chain.Id));
    }

    [Fact]
    public async Task Cree_une_chaine_au_nouveau_format_sans_remplir_les_anciennes_colonnes()
    {
        var admin = await _api.CreateClientAsAsync(UserRole.Admin);

        var response = await admin.PostAsJsonAsync(ChainsApi.Path, ChainsApi.MeterBody(ChainsApi.UniqueName()));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var chain = await response.Content.ReadFromJsonAsync<AdminChainDto>();
        Assert.NotNull(chain);
        Assert.Null(chain.Cost);
        Assert.Null(chain.Price);
        Assert.Null(chain.Length);
        Assert.Equal(4m, chain.CostPerMeter);
        Assert.Equal(10m, chain.PricePerMeter);
        Assert.Equal(1.5m, chain.ClaspCost);
        Assert.Equal(3m, chain.ClaspPrice);
        Assert.Equal(250, chain.StockCm);
        Assert.Equal([250], await RollLengthsAsync(chain.Id));
    }

    [Fact]
    public async Task Refuse_une_chaine_sans_prix_avec_400()
    {
        var admin = await _api.CreateClientAsAsync(UserRole.Admin);

        var response = await admin.PostAsJsonAsync(ChainsApi.Path, new
        {
            Name = ChainsApi.UniqueName(),
            Description = "Maille fine",
            Color = ProductColor.Gold
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ApiError>();
        Assert.NotNull(error?.Errors);
        Assert.Contains("Le coût au mètre est requis.", Assert.Contains("CostPerMeter", error.Errors));
        Assert.Contains("Le prix au mètre est requis.", Assert.Contains("PricePerMeter", error.Errors));
    }

    [Fact]
    public async Task Refuse_la_creation_a_un_utilisateur_non_admin_avec_403()
    {
        var user = await _api.CreateClientAsAsync(UserRole.User);

        var response = await user.PostAsJsonAsync(ChainsApi.Path, ChainsApi.MeterBody(ChainsApi.UniqueName()));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private Task<List<int>> RollLengthsAsync(long chainId) =>
        _api.QueryAsync(services => services.GetRequiredService<AppDbContext>().Chains
            .Where(c => c.Id == chainId)
            .SelectMany(c => c.Rolls)
            .Select(r => r.RemainingLengthCm)
            .ToListAsync());
}

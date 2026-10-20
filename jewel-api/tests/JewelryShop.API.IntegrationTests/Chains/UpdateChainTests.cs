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
public class UpdateChainTests
{
    private readonly JewelryShopApiFactory _api;

    public UpdateChainTests(JewelryShopApiFactory api) => _api = api;

    [Fact]
    public async Task Recalcule_les_prix_au_metre_a_l_ancien_format_sans_toucher_aux_rouleaux_ni_au_fermoir()
    {
        var admin = await _api.CreateClientAsAsync(UserRole.Admin);
        var created = await admin.CreateChainAsync(ChainsApi.MeterBody(ChainsApi.UniqueName()));

        var response = await admin.PutAsJsonAsync(
            $"{ChainsApi.Path}/{created.Id}",
            ChainsApi.LegacyBody(created.Name, cost: 15m, price: 60m, length: 300m));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var chain = await response.Content.ReadFromJsonAsync<AdminChainDto>();
        Assert.NotNull(chain);
        Assert.Equal(60m, chain.Price);
        Assert.Equal(5m, chain.CostPerMeter);
        Assert.Equal(20m, chain.PricePerMeter);
        Assert.Equal(1.5m, chain.ClaspCost);
        Assert.Equal(3m, chain.ClaspPrice);
        Assert.Equal(250, chain.StockCm);
        Assert.Equal(1, await RollCountAsync(created.Id));
    }

    [Fact]
    public async Task Garde_les_anciennes_colonnes_au_nouveau_format()
    {
        var admin = await _api.CreateClientAsAsync(UserRole.Admin);
        var created = await admin.CreateChainAsync(ChainsApi.LegacyBody(ChainsApi.UniqueName()));

        var response = await admin.PutAsJsonAsync($"{ChainsApi.Path}/{created.Id}", ChainsApi.MeterBody(created.Name));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var chain = await response.Content.ReadFromJsonAsync<AdminChainDto>();
        Assert.NotNull(chain);
        Assert.Equal(20m, chain.Cost);
        Assert.Equal(50m, chain.Price);
        Assert.Equal(500m, chain.Length);
        Assert.Equal(4m, chain.CostPerMeter);
        Assert.Equal(10m, chain.PricePerMeter);
        Assert.Equal(1.5m, chain.ClaspCost);
        Assert.Equal(500, chain.StockCm);
        Assert.Equal(1, await RollCountAsync(created.Id));
    }

    private Task<int> RollCountAsync(long chainId) =>
        _api.QueryAsync(services => services.GetRequiredService<AppDbContext>().Chains
            .Where(c => c.Id == chainId)
            .SelectMany(c => c.Rolls)
            .CountAsync());
}

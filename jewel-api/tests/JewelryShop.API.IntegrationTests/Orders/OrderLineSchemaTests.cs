using JewelryShop.API.IntegrationTests.Chains;
using JewelryShop.API.IntegrationTests.Charms;
using JewelryShop.API.IntegrationTests.Infrastructure;
using JewelryShop.Common.Enums;
using JewelryShop.Features.Orders.Entities;
using Microsoft.EntityFrameworkCore;

namespace JewelryShop.API.IntegrationTests.Orders;

/// <summary>Contrainte <c>chk_order_lines_kind_product</c> de V11 : le type de ligne et le produit référencé concordent.</summary>
[Collection(ApiCollection.Name)]
public class OrderLineSchemaTests
{
    private readonly JewelryShopApiFactory _api;

    public OrderLineSchemaTests(JewelryShopApiFactory api) => _api = api;

    [Theory]
    [InlineData(OrderLineKind.Chain, false, false)]
    [InlineData(OrderLineKind.Chain, true, true)]
    [InlineData(OrderLineKind.Chain, false, true)]
    [InlineData(OrderLineKind.Charm, false, false)]
    [InlineData(OrderLineKind.Charm, true, true)]
    [InlineData(OrderLineKind.Charm, true, false)]
    public async Task Refuse_une_ligne_dont_le_produit_ne_correspond_pas_a_son_type(
        OrderLineKind kind, bool withChain, bool withCharm)
    {
        var admin = await _api.CreateClientAsAsync(UserRole.Admin);
        var chainId = (await admin.CreateChainAsync(ChainsApi.MeterBody(ChainsApi.UniqueName()))).Id;
        var charmId = await admin.CreateCharmAsync();
        var line = new OrderLine
        {
            Kind = kind,
            ChainId = withChain ? chainId : null,
            CharmId = withCharm ? charmId : null,
            Name = "Ligne incohérente",
            UnitPrice = 5m
        };

        await Assert.ThrowsAsync<DbUpdateException>(() => _api.SeedOrderAsync(line));
    }
}

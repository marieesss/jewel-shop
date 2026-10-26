using System.Text.Json;
using JewelryShop.API.Tests.Fakes;
using JewelryShop.Common;
using JewelryShop.Common.Enums;
using JewelryShop.Features.Chains.DTOs;
using JewelryShop.Features.Chains.Entities;
using JewelryShop.Features.Chains.Queries;

namespace JewelryShop.API.Tests.Chains;

public class GetChainByIdQueryHandlerTests
{
    private static readonly JsonSerializerOptions _web = new(JsonSerializerDefaults.Web);

    private readonly FakeChainRepository _chains = new();
    private readonly GetChainByIdQueryHandler _handler;

    public GetChainByIdQueryHandlerTests()
    {
        _handler = new GetChainByIdQueryHandler(_chains);
        _chains.Seed(Chain());
    }

    private static Chain Chain()
    {
        var chain = new Chain
        {
            Id = 7,
            Name = "Forçat",
            Color = ProductColor.Gold,
            Url = "https://fournisseur.example/forcat",
            Cost = 12m,
            Price = 30m,
            Length = 300m,
            CostPerMeter = 4m,
            PricePerMeter = 10m,
            ClaspCost = 1m,
            ClaspPrice = 3m,
        };
        chain.AddRoll(300, new DateTime(2026, 10, 19, 8, 0, 0, DateTimeKind.Utc));
        return chain;
    }

    [Fact]
    public async Task Renvoie_la_chaine_sans_lien_ni_couts_a_un_client()
    {
        var dto = await _handler.Handle(new GetChainByIdQuery(7), CancellationToken.None);

        Assert.IsType<ChainDto>(dto);
        Assert.Equal(new ChainDto(7, "Forçat", "", "Gold", null, 30m, 300m, 10m, 3m, 300), dto);
    }

    [Fact]
    public async Task Renvoie_le_lien_et_les_couts_a_un_admin()
    {
        var dto = await _handler.Handle(new GetChainByIdQuery(7, IncludeAdminFields: true), CancellationToken.None);

        var admin = Assert.IsType<AdminChainDto>(dto);
        Assert.Equal("https://fournisseur.example/forcat", admin.Url);
        Assert.Equal(12m, admin.Cost);
        Assert.Equal(4m, admin.CostPerMeter);
        Assert.Equal(1m, admin.ClaspCost);
        Assert.Equal(3m, admin.ClaspPrice);
        Assert.Equal(300, admin.StockCm);
    }

    [Fact]
    public async Task Garde_les_champs_admin_a_la_serialisation_d_une_liste_typee_en_dto_public()
    {
        var dto = await _handler.Handle(new GetChainByIdQuery(7, IncludeAdminFields: true), CancellationToken.None);
        var page = new PagedResult<ChainDto>([dto], 1, 1, 20);

        var item = JsonSerializer.SerializeToElement(page, _web).GetProperty("items")[0];

        Assert.Equal("https://fournisseur.example/forcat", item.GetProperty("url").GetString());
        Assert.Equal(12m, item.GetProperty("cost").GetDecimal());
        Assert.Equal(4m, item.GetProperty("costPerMeter").GetDecimal());
        Assert.Equal(1m, item.GetProperty("claspCost").GetDecimal());
    }

    [Fact]
    public async Task Ne_serialise_ni_lien_ni_couts_pour_un_client()
    {
        var dto = await _handler.Handle(new GetChainByIdQuery(7), CancellationToken.None);

        var json = JsonSerializer.SerializeToElement(dto, _web);

        Assert.False(json.TryGetProperty("url", out _));
        Assert.False(json.TryGetProperty("cost", out _));
        Assert.False(json.TryGetProperty("costPerMeter", out _));
        Assert.False(json.TryGetProperty("claspCost", out _));
        Assert.Equal(3m, json.GetProperty("claspPrice").GetDecimal());
    }
}

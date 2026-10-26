using System.Text.Json;
using JewelryShop.API.Tests.Fakes;
using JewelryShop.Common;
using JewelryShop.Common.Enums;
using JewelryShop.Features.Charms.DTOs;
using JewelryShop.Features.Charms.Entities;
using JewelryShop.Features.Charms.Queries;

namespace JewelryShop.API.Tests.Charms;

public class GetCharmByIdQueryHandlerTests
{
    private static readonly JsonSerializerOptions _web = new(JsonSerializerDefaults.Web);

    private readonly FakeCharmRepository _charms = new();
    private readonly GetCharmByIdQueryHandler _handler;

    public GetCharmByIdQueryHandlerTests()
    {
        _handler = new GetCharmByIdQueryHandler(_charms);
        _charms.Seed(new Charm
        {
            Id = 7,
            Name = "Trèfle",
            Color = ProductColor.Silver,
            Url = "https://fournisseur.example/trefle",
            Cost = 2m,
            Price = 8m,
            Stock = 10,
        });
    }

    [Fact]
    public async Task Renvoie_la_breloque_sans_lien_ni_cout_a_un_client()
    {
        var dto = await _handler.Handle(new GetCharmByIdQuery(7), CancellationToken.None);

        Assert.IsType<CharmDto>(dto);
        Assert.Equal(new CharmDto(7, "Trèfle", "", "Silver", null, 8m, 10), dto);
    }

    [Fact]
    public async Task Renvoie_le_lien_et_le_cout_a_un_admin()
    {
        var dto = await _handler.Handle(new GetCharmByIdQuery(7, IncludeAdminFields: true), CancellationToken.None);

        var admin = Assert.IsType<AdminCharmDto>(dto);
        Assert.Equal("https://fournisseur.example/trefle", admin.Url);
        Assert.Equal(2m, admin.Cost);
        Assert.Equal(8m, admin.Price);
    }

    [Fact]
    public async Task Garde_les_champs_admin_a_la_serialisation_d_une_liste_typee_en_dto_public()
    {
        var dto = await _handler.Handle(new GetCharmByIdQuery(7, IncludeAdminFields: true), CancellationToken.None);
        var page = new PagedResult<CharmDto>([dto], 1, 1, 20);

        var item = JsonSerializer.SerializeToElement(page, _web).GetProperty("items")[0];

        Assert.Equal("https://fournisseur.example/trefle", item.GetProperty("url").GetString());
        Assert.Equal(2m, item.GetProperty("cost").GetDecimal());
    }

    [Fact]
    public async Task Ne_serialise_ni_lien_ni_cout_pour_un_client()
    {
        var dto = await _handler.Handle(new GetCharmByIdQuery(7), CancellationToken.None);

        var json = JsonSerializer.SerializeToElement(dto, _web);

        Assert.False(json.TryGetProperty("url", out _));
        Assert.False(json.TryGetProperty("cost", out _));
        Assert.Equal(8m, json.GetProperty("price").GetDecimal());
    }
}

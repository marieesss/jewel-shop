using JewelryShop.API.Tests.Fakes;
using JewelryShop.Common.Enums;
using JewelryShop.Common.Exceptions;
using JewelryShop.Features.ChainLengths.Entities;
using JewelryShop.Features.Chains.Entities;
using JewelryShop.Features.Charms.Entities;
using JewelryShop.Features.Orders.Common;
using JewelryShop.Features.Orders.DTOs;
using JewelryShop.Features.Orders.Queries;

namespace JewelryShop.API.Tests.Orders;

public class GetQuoteQueryHandlerTests
{
    private readonly FakeChainRepository _chains = new();
    private readonly FakeChainLengthRepository _lengths = new();
    private readonly FakeCharmRepository _charms = new();
    private readonly GetQuoteQueryHandler _handler;

    public GetQuoteQueryHandlerTests()
    {
        _handler = new GetQuoteQueryHandler(new NecklaceComposer(_chains, _lengths, _charms));

        var chain = new Chain { Id = 3, Name = "Forçat", Color = ProductColor.Gold, PricePerMeter = 10m, ClaspPrice = 2m };
        chain.AddRoll(250, new DateTime(2026, 11, 2, 8, 0, 0, DateTimeKind.Utc));
        _chains.Seed(chain);
        _lengths.Seed(new ChainLength { Id = 1, LengthCm = 45, Label = "Princesse" });
        _charms.Seed(
            new Charm { Id = 10, Name = "Trèfle", Color = ProductColor.Silver, Price = 5m, Stock = 2 },
            new Charm { Id = 11, Name = "Cœur", Color = ProductColor.Silver, Price = 5m, Stock = 5 });
    }

    [Fact]
    public async Task Calcule_un_total_de_16_50_pour_une_chaine_de_45_cm_et_deux_breloques_a_5_euros()
    {
        var quote = await _handler.Handle(new GetQuoteQuery(3, 45, [11, 10]), CancellationToken.None);

        Assert.Equal(16.50m, quote.Total);
    }

    [Fact]
    public async Task Renvoie_la_chaine_puis_les_breloques_avec_leur_position()
    {
        var quote = await _handler.Handle(new GetQuoteQuery(3, 45, [11, 10]), CancellationToken.None);

        Assert.Equal(
            [
                new QuoteLineDto("Chain", 3, "Forçat", 45, null, 6.50m),
                new QuoteLineDto("Charm", 11, "Cœur", null, 1, 5m),
                new QuoteLineDto("Charm", 10, "Trèfle", null, 2, 5m),
            ],
            quote.Lines);
    }

    [Fact]
    public async Task Refuse_3_exemplaires_d_une_breloque_dont_le_stock_est_2()
    {
        var error = await Assert.ThrowsAsync<ConflictException>(
            () => _handler.Handle(new GetQuoteQuery(3, 45, [10, 10, 10]), CancellationToken.None));

        Assert.Equal("Le stock de « Trèfle » ne permet plus cette commande.", error.Message);
    }
}

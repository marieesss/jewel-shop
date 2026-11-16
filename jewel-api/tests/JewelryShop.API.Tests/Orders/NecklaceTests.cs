using JewelryShop.Common.Enums;
using JewelryShop.Features.Chains.Entities;
using JewelryShop.Features.Orders.Common;

namespace JewelryShop.API.Tests.Orders;

public class NecklaceTests
{
    private static Chain Chain(decimal? pricePerMeter, int rollLengthCm = 50)
    {
        var chain = new Chain
        {
            Id = 3,
            Name = "Forçat",
            Color = ProductColor.Gold,
            PricePerMeter = pricePerMeter,
            ClaspPrice = 2m,
        };
        chain.AddRoll(rollLengthCm, new DateTime(2026, 11, 2, 8, 0, 0, DateTimeKind.Utc));
        return chain;
    }

    [Fact]
    public void Calcule_le_prix_de_la_chaine_fermoir_compris()
    {
        var necklace = new Necklace(Chain(pricePerMeter: 10m), 45, []);

        Assert.Equal(6.50m, necklace.ChainPrice);
    }

    [Fact]
    public void Refuse_une_chaine_sans_prix_au_metre()
    {
        Assert.Throws<ArgumentException>(() => new Necklace(Chain(pricePerMeter: null), 45, []));
    }

    [Fact]
    public void Refuse_une_longueur_qu_aucun_rouleau_ne_couvre()
    {
        Assert.Throws<ArgumentException>(() => new Necklace(Chain(pricePerMeter: 10m, rollLengthCm: 40), 45, []));
    }
}

using JewelryShop.Features.Chains.Common;

namespace JewelryShop.API.Tests.Chains;

public class ChainPricingTests
{
    [Theory]
    [InlineData(50, 500, 10.00)]
    [InlineData(20, 500, 4.00)]
    [InlineData(39.90, 45, 88.67)]
    [InlineData(0, 300, 0)]
    public void Convertit_un_montant_total_en_montant_au_metre(decimal total, decimal lengthCm, decimal expected)
    {
        Assert.Equal(expected, ChainPricing.PerMeter(total, lengthCm));
    }

    [Theory]
    [InlineData(0.00125, 0.12)]
    [InlineData(0.00135, 0.14)]
    public void Arrondit_au_pair_un_montant_exactement_au_milieu_de_deux_centimes(decimal total, decimal expected)
    {
        Assert.Equal(expected, ChainPricing.PerMeter(total, 1m));
    }

    [Fact]
    public void Accepte_une_longueur_decimale()
    {
        Assert.Equal(10.00m, ChainPricing.PerMeter(12.55m, 125.5m));
    }

    [Theory]
    [InlineData(300, 300)]
    [InlineData(45.49, 45)]
    [InlineData(45.5, 46)]
    [InlineData(46.5, 47)]
    [InlineData(0.4, 0)]
    public void Arrondit_la_longueur_du_rouleau_au_demi_superieur(decimal lengthCm, int expected)
    {
        Assert.Equal(expected, ChainPricing.RollLengthCm(lengthCm));
    }

    [Theory]
    [InlineData(10, 45, 2, 6.50)]
    [InlineData(10, 81, 2, 10.10)]
    [InlineData(39.90, 45, 0, 17.96)]
    [InlineData(0, 50, 3, 3.00)]
    public void Calcule_le_prix_d_une_longueur_fermoir_compris(
        decimal pricePerMeter, int lengthCm, decimal claspPrice, decimal expected)
    {
        Assert.Equal(expected, ChainPricing.PriceFor(pricePerMeter, lengthCm, claspPrice));
    }

    [Theory]
    [InlineData(0.5, 1, 0.00)]
    [InlineData(1.5, 1, 0.02)]
    [InlineData(10.05, 50, 5.02)]
    [InlineData(10.15, 50, 5.08)]
    public void Arrondit_au_pair_le_prix_d_une_longueur_au_demi_centime(
        decimal pricePerMeter, int lengthCm, decimal expected)
    {
        Assert.Equal(expected, ChainPricing.PriceFor(pricePerMeter, lengthCm, 0m));
    }
}

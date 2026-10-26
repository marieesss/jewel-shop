using JewelryShop.Common.Enums;
using JewelryShop.Features.ChainLengths.Entities;
using JewelryShop.Features.Chains.DTOs;
using JewelryShop.Features.Chains.Entities;

namespace JewelryShop.API.Tests.Chains;

public class ChainMappingExtensionsTests
{
    private static readonly DateTime _rollDate = new(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc);

    private static readonly ChainLength[] _referenceLengths =
    [
        new() { Id = 9, LengthCm = 81, Label = "Sautoir" },
        new() { Id = 1, LengthCm = 38, Label = "Ras du cou" },
        new() { Id = 2, LengthCm = 42, Label = "Court" },
        new() { Id = 3, LengthCm = 45, Label = "Princesse" },
        new() { Id = 4, LengthCm = 50, Label = "Mi-long" },
        new() { Id = 5, LengthCm = 55, Label = "Matinée" },
        new() { Id = 6, LengthCm = 60, Label = "Long" },
        new() { Id = 7, LengthCm = 65, Label = "Opéra court" },
        new() { Id = 8, LengthCm = 70, Label = "Opéra" }
    ];

    private static Chain ChainWithRolls(decimal? pricePerMeter, params int[] rollLengthsCm)
    {
        var chain = new Chain { Id = 1, Name = "Forçat", Color = ProductColor.Gold, PricePerMeter = pricePerMeter, ClaspPrice = 2m };
        foreach (var lengthCm in rollLengthsCm)
            chain.AddRoll(lengthCm, _rollDate);
        return chain;
    }

    private static ChainLengthPriceDto LengthOf(ChainDto dto, int lengthCm) =>
        Assert.Single(dto.AvailableLengths!, l => l.LengthCm == lengthCm);

    [Fact]
    public void Calcule_le_prix_de_chaque_longueur_fermoir_compris()
    {
        var dto = ChainWithRolls(10m, 100).ToDto(false, _referenceLengths);

        Assert.Equal(new ChainLengthPriceDto(45, "Princesse", 6.50m, true), LengthOf(dto, 45));
        Assert.Equal(new ChainLengthPriceDto(81, "Sautoir", 10.10m, true), LengthOf(dto, 81));
    }

    [Fact]
    public void Renvoie_toutes_les_longueurs_de_reference_triees()
    {
        var dto = ChainWithRolls(10m, 100).ToDto(false, _referenceLengths);

        Assert.Equal([38, 42, 45, 50, 55, 60, 65, 70, 81], dto.AvailableLengths!.Select(l => l.LengthCm));
    }

    [Fact]
    public void Rend_disponibles_les_longueurs_couvertes_par_un_rouleau_seulement()
    {
        var dto = ChainWithRolls(10m, 50).ToDto(false, _referenceLengths);

        Assert.Equal([38, 42, 45, 50], dto.AvailableLengths!.Where(l => l.Available).Select(l => l.LengthCm));
        Assert.Equal([55, 60, 65, 70, 81], dto.AvailableLengths!.Where(l => !l.Available).Select(l => l.LengthCm));
    }

    [Fact]
    public void N_additionne_pas_deux_rouleaux_pour_couvrir_une_longueur()
    {
        var dto = ChainWithRolls(10m, 40, 40).ToDto(false, _referenceLengths);

        Assert.True(LengthOf(dto, 38).Available);
        Assert.False(LengthOf(dto, 42).Available);
    }

    [Fact]
    public void Rend_toutes_les_longueurs_indisponibles_sans_rouleau()
    {
        var dto = ChainWithRolls(10m).ToDto(false, _referenceLengths);

        Assert.All(dto.AvailableLengths!, l => Assert.False(l.Available));
        Assert.Null(dto.FromPrice);
    }

    [Fact]
    public void Renvoie_le_prix_de_la_moins_chere_des_longueurs_disponibles()
    {
        var dto = ChainWithRolls(10m, 30, 60).ToDto(false, _referenceLengths);

        Assert.Equal(5.80m, dto.FromPrice);
    }

    [Fact]
    public void Ignore_les_longueurs_indisponibles_dans_le_prix_a_partir_de()
    {
        var lengths = new[] { new ChainLength { Id = 1, LengthCm = 38, Label = "Ras du cou" } };

        var dto = ChainWithRolls(10m, 20).ToDto(false, lengths);

        Assert.Equal(5.80m, LengthOf(dto, 38).Price);
        Assert.Null(dto.FromPrice);
    }

    [Fact]
    public void Ne_vend_aucune_longueur_d_une_chaine_sans_prix_au_metre()
    {
        var dto = ChainWithRolls(null, 100).ToDto(false, _referenceLengths);

        Assert.All(dto.AvailableLengths!, l =>
        {
            Assert.Null(l.Price);
            Assert.False(l.Available);
        });
        Assert.Null(dto.FromPrice);
    }

    [Fact]
    public void Ne_calcule_pas_les_longueurs_sans_la_table_de_reference()
    {
        var dto = ChainWithRolls(10m, 100).ToDto();

        Assert.Null(dto.AvailableLengths);
        Assert.Null(dto.FromPrice);
    }
}

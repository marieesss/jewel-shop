using JewelryShop.Features.Tags.Common;

namespace JewelryShop.API.Tests.Tags;

public class TagSlugTests
{
    [Theory]
    [InlineData("Mer", "mer")]
    [InlineData("MER", "mer")]
    [InlineData("Doré", "dore")]
    [InlineData("Été", "ete")]
    [InlineData("Ça brille", "ca-brille")]
    public void Met_en_minuscules_et_retire_les_accents(string name, string expected)
    {
        Assert.Equal(expected, TagSlug.From(name));
    }

    [Theory]
    [InlineData("Fête des mères", "fete-des-meres")]
    [InlineData("  Bord de   mer  ", "bord-de-mer")]
    public void Remplace_les_espaces_par_un_seul_tiret(string name, string expected)
    {
        Assert.Equal(expected, TagSlug.From(name));
    }

    [Theory]
    [InlineData("L'été", "l-ete")]
    [InlineData("Saint-Valentin", "saint-valentin")]
    [InlineData("Noir & blanc", "noir-blanc")]
    [InlineData("!Fleurs!", "fleurs")]
    [InlineData("Années 80", "annees-80")]
    public void Remplace_les_autres_caracteres_par_un_tiret_sans_tiret_double_ni_en_bordure(string name, string expected)
    {
        Assert.Equal(expected, TagSlug.From(name));
    }

    [Theory]
    [InlineData("Cœur", "coeur")]
    [InlineData("Ex æquo", "ex-aequo")]
    public void Ecrit_les_ligatures_en_deux_lettres(string name, string expected)
    {
        Assert.Equal(expected, TagSlug.From(name));
    }

    [Fact]
    public void Coupe_un_slug_trop_long_sans_finir_par_un_tiret()
    {
        var name = new string('œ', 29) + "a bijou";

        var slug = TagSlug.From(name);

        Assert.Equal(string.Concat(Enumerable.Repeat("oe", 29)) + "a", slug);
        Assert.True(slug.Length <= TagRules.SlugMaxLength);
    }

    [Theory]
    [InlineData("!!!")]
    [InlineData("  ")]
    public void Donne_un_slug_vide_sans_lettre_ni_chiffre(string name)
    {
        Assert.Equal("", TagSlug.From(name));
        Assert.False(TagSlug.CanBeBuiltFrom(name));
    }
}

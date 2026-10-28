using JewelryShop.Features.Tags.Common;
using JewelryShop.Features.Tags.Entities;
using static JewelryShop.API.Tests.Tags.TagFixtures;

namespace JewelryShop.API.Tests.Tags;

public class TagFilterTests
{
    [Fact]
    public void Regroupe_les_tags_d_une_meme_categorie_et_separe_les_categories()
    {
        var filter = TagFilter.From([Tag(3, "Doré", TagCategory.Color), Tag(2, "Plage"), Tag(1, "Mer")]);

        Assert.Equal([[1L, 2L], [3L]], filter.TagIdGroups);
    }

    [Fact]
    public void Ordonne_les_groupes_dans_l_ordre_des_categories()
    {
        var filter = TagFilter.From([Tag(1, "Mariage", TagCategory.Occasion), Tag(2, "Or", TagCategory.Material), Tag(3, "Mer")]);

        Assert.Equal([[3L], [2L], [1L]], filter.TagIdGroups);
    }

    [Fact]
    public void Ne_filtre_rien_sans_tag()
    {
        var filter = TagFilter.From([]);

        Assert.True(filter.IsEmpty);
    }

    [Fact]
    public void Lit_les_slugs_separes_par_des_virgules()
    {
        Assert.Equal(["mer", "plage", "dore"], TagFilter.ParseSlugs("mer,plage,dore"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(",, ,")]
    public void Ne_lit_aucun_slug_d_un_parametre_vide(string? tags)
    {
        Assert.Empty(TagFilter.ParseSlugs(tags));
    }

    [Fact]
    public void Ramene_chaque_valeur_a_un_slug_et_retire_les_doublons()
    {
        Assert.Equal(["dore", "mer"], TagFilter.ParseSlugs(" Doré , mer,,dore,MER"));
    }
}

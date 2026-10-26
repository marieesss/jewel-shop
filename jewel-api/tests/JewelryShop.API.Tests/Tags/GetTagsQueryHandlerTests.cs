using JewelryShop.API.Tests.Fakes;
using JewelryShop.Features.Tags.DTOs;
using JewelryShop.Features.Tags.Entities;
using JewelryShop.Features.Tags.Queries;
using static JewelryShop.API.Tests.Tags.TagFixtures;

namespace JewelryShop.API.Tests.Tags;

public class GetTagsQueryHandlerTests
{
    private readonly FakeTagRepository _tags = new();

    [Fact]
    public async Task Renvoie_les_tags_tries_par_categorie_dans_l_ordre_de_l_enum_puis_par_nom()
    {
        _tags.Seed(
            Tag(1, "Mariage", TagCategory.Occasion),
            Tag(2, "Or", TagCategory.Material),
            Tag(3, "Mer", TagCategory.Theme),
            Tag(4, "Doré", TagCategory.Color),
            Tag(5, "fleurs", TagCategory.Theme),
            Tag(6, "Argenté", TagCategory.Color));
        var handler = new GetTagsQueryHandler(_tags);

        var result = await handler.Handle(new GetTagsQuery(), CancellationToken.None);

        Assert.Equal(
            [
                new TagDto(5, "fleurs", "fleurs", "Theme"),
                new TagDto(3, "Mer", "mer", "Theme"),
                new TagDto(6, "Argenté", "argente", "Color"),
                new TagDto(4, "Doré", "dore", "Color"),
                new TagDto(2, "Or", "or", "Material"),
                new TagDto(1, "Mariage", "mariage", "Occasion")
            ],
            result);
    }

    [Fact]
    public async Task Renvoie_une_liste_vide_sans_tag()
    {
        var handler = new GetTagsQueryHandler(_tags);

        var result = await handler.Handle(new GetTagsQuery(), CancellationToken.None);

        Assert.Empty(result);
    }
}

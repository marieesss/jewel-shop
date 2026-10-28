using JewelryShop.Features.Tags.Common;
using JewelryShop.Features.Tags.Queries;
using JewelryShop.Features.Tags.Validators;

namespace JewelryShop.API.Tests.Tags;

public class GetTagCountsQueryValidatorTests
{
    private readonly GetTagCountsQueryValidator _validator = new();

    [Theory]
    [InlineData("chain")]
    [InlineData("charm")]
    public void Accepte_un_type_d_article_connu(string kind)
    {
        Assert.True(_validator.Validate(new GetTagCountsQuery(kind)).IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("Charm")]
    [InlineData("charms")]
    [InlineData("creation")]
    public void Refuse_un_type_d_article_inconnu(string kind)
    {
        var result = _validator.Validate(new GetTagCountsQuery(kind));

        ValidationAssert.HasError(result, nameof(GetTagCountsQuery.Kind), TagMessages.InvalidKind);
    }
}

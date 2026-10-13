using JewelryShop.Application.Features.Favorites.Commands;

namespace JewelryShop.API.Tests.Favorites;

public class AddFavoriteCommandValidatorTests
{
    private const string ExactlyOneProductMessage =
        "Renseignez exactement un produit : chainId ou charmId, mais pas les deux.";

    private readonly AddFavoriteCommandValidator _validator = new();

    [Theory]
    [InlineData(3L, null)]
    [InlineData(null, 5L)]
    public void Accepte_un_favori_avec_un_seul_produit(long? chainId, long? charmId)
    {
        var result = _validator.Validate(new AddFavoriteCommand(1, chainId, charmId));

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData(3L, 5L)]
    public void Refuse_un_favori_sans_produit_ou_avec_deux_produits(long? chainId, long? charmId)
    {
        var result = _validator.Validate(new AddFavoriteCommand(1, chainId, charmId));

        var error = Assert.Single(result.Errors);
        Assert.Equal(ExactlyOneProductMessage, error.ErrorMessage);
    }

    [Fact]
    public void Refuse_un_identifiant_utilisateur_invalide()
    {
        var result = _validator.Validate(new AddFavoriteCommand(0, 3, null));

        var error = Assert.Single(result.Errors);
        Assert.Equal(nameof(AddFavoriteCommand.UserId), error.PropertyName);
        Assert.Equal("Identifiant invalide.", error.ErrorMessage);
    }
}

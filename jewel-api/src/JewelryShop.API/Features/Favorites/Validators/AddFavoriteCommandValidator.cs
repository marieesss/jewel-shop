using FluentValidation;
using JewelryShop.Features.Favorites.Commands;

namespace JewelryShop.Features.Favorites.Validators;

public class AddFavoriteCommandValidator : AbstractValidator<AddFavoriteCommand>
{
    public AddFavoriteCommandValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0).WithMessage("Identifiant invalide.");

        RuleFor(x => x)
            .Must(x => x.ChainId.HasValue ^ x.CharmId.HasValue)
            .WithMessage("Renseignez exactement un produit : chainId ou charmId, mais pas les deux.");
    }
}

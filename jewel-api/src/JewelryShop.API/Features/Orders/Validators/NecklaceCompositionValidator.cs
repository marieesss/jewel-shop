using FluentValidation;
using JewelryShop.Features.Orders.Common;

namespace JewelryShop.Features.Orders.Validators;

/// <summary>
/// Règles de forme d'un collier, communes au devis et à la commande. Les règles qui demandent la base
/// (produits existants, longueur proposée, stock) sont vérifiées par <see cref="NecklaceComposer"/>.
/// </summary>
public class NecklaceCompositionValidator<T> : AbstractValidator<T> where T : INecklaceComposition
{
    public NecklaceCompositionValidator()
    {
        RuleFor(x => x.ChainId)
            .GreaterThan(0).WithMessage(NecklaceMessages.InvalidId);

        RuleFor(x => x.CharmIds)
            .NotEmpty().WithMessage(NecklaceMessages.CharmRequired)
            .Must(ids => ids is null || ids.Count <= NecklaceRules.MaxCharms).WithMessage(NecklaceMessages.TooManyCharms);

        RuleForEach(x => x.CharmIds)
            .GreaterThan(0).WithMessage(NecklaceMessages.InvalidId);
    }
}

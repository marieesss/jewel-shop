using FluentValidation;
using JewelryShop.Features.Chains.Common;

namespace JewelryShop.Features.Chains.Validators;

/// <summary>
/// Règles des prix d'une chaîne, communes à la création et à la modification.
/// </summary>
public class ChainPricesValidator<T> : AbstractValidator<T> where T : IChainPrices
{
    public const string CostMessage = "Le coût doit être positif.";
    public const string PriceMessage = "Le prix doit être positif.";

    public ChainPricesValidator()
    {
        When(x => x.UsesMeterPrices() || !x.HasCompleteLegacyPrices(), () =>
        {
            RuleFor(x => x.CostPerMeter)
                .NotNull().WithMessage("Le coût au mètre est requis.");
            RuleFor(x => x.PricePerMeter)
                .NotNull().WithMessage("Le prix au mètre est requis.");
        });

        RuleFor(x => x.CostPerMeter)
            .GreaterThanOrEqualTo(0).WithMessage(CostMessage);
        RuleFor(x => x.PricePerMeter)
            .GreaterThanOrEqualTo(0).WithMessage(PriceMessage);
        RuleFor(x => x.ClaspCost)
            .GreaterThanOrEqualTo(0).WithMessage(CostMessage);
        RuleFor(x => x.ClaspPrice)
            .GreaterThanOrEqualTo(0).WithMessage(PriceMessage);
        RuleFor(x => x.Cost)
            .GreaterThanOrEqualTo(0).WithMessage(CostMessage);
        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage(PriceMessage);
        RuleFor(x => x.Length)
            .GreaterThan(0).WithMessage("La longueur doit être supérieure à 0.");
    }
}

using FluentValidation;
using JewelryShop.Features.ChainLengths.Commands;

namespace JewelryShop.Features.ChainLengths.Validators;

public class UpdateChainLengthCommandValidator : AbstractValidator<UpdateChainLengthCommand>
{
    public UpdateChainLengthCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Identifiant invalide.");

        Include(new ChainLengthFieldsValidator<UpdateChainLengthCommand>());
    }
}

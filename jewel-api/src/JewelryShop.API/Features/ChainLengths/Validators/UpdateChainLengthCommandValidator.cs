using FluentValidation;
using JewelryShop.Features.ChainLengths.Commands;
using JewelryShop.Features.ChainLengths.Common;

namespace JewelryShop.Features.ChainLengths.Validators;

public class UpdateChainLengthCommandValidator : AbstractValidator<UpdateChainLengthCommand>
{
    public UpdateChainLengthCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage(ChainLengthMessages.InvalidId);

        Include(new ChainLengthFieldsValidator<UpdateChainLengthCommand>());
    }
}

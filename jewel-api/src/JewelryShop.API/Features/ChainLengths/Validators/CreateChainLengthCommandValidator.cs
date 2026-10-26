using FluentValidation;
using JewelryShop.Features.ChainLengths.Commands;

namespace JewelryShop.Features.ChainLengths.Validators;

public class CreateChainLengthCommandValidator : AbstractValidator<CreateChainLengthCommand>
{
    public CreateChainLengthCommandValidator()
    {
        Include(new ChainLengthFieldsValidator<CreateChainLengthCommand>());
    }
}

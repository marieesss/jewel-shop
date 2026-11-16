using FluentValidation;
using JewelryShop.Features.Orders.Queries;

namespace JewelryShop.Features.Orders.Validators;

public class GetQuoteQueryValidator : AbstractValidator<GetQuoteQuery>
{
    public GetQuoteQueryValidator()
    {
        Include(new NecklaceCompositionValidator<GetQuoteQuery>());
    }
}

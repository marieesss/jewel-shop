using FluentValidation;
using JewelryShop.Features.Orders.Common;
using JewelryShop.Features.Orders.Queries;

namespace JewelryShop.Features.Orders.Validators;

public class GetOrdersQueryValidator : AbstractValidator<GetOrdersQuery>
{
    public GetOrdersQueryValidator()
    {
        RuleFor(x => x.Status)
            .Must(status => OrderStatusFilter.TryParse(status, out _))
            .WithMessage(OrderMessages.InvalidStatus);
    }
}

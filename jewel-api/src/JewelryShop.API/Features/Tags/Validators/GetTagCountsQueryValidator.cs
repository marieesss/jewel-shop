using FluentValidation;
using JewelryShop.Features.Tags.Common;
using JewelryShop.Features.Tags.Queries;

namespace JewelryShop.Features.Tags.Validators;

public class GetTagCountsQueryValidator : AbstractValidator<GetTagCountsQuery>
{
    public GetTagCountsQueryValidator()
    {
        RuleFor(x => x.Kind)
            .Must(TagCountKinds.IsValid).WithMessage(TagMessages.InvalidKind);
    }
}

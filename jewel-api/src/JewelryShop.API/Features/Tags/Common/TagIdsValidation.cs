using System.Linq.Expressions;
using FluentValidation;

namespace JewelryShop.Features.Tags.Common;

public static class TagIdsValidation
{
    /// <summary>Chaque id de <c>tagIds</c> doit être positif. Il n'y a pas de maximum : le nombre de tags par article est libre.</summary>
    public static void RuleForTagIds<T>(this AbstractValidator<T> validator, Expression<Func<T, IEnumerable<long>?>> tagIds) =>
        validator.RuleForEach(tagIds)
            .GreaterThan(0).WithMessage(TagMessages.InvalidId);
}

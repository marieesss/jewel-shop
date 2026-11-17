using FluentValidation;
using JewelryShop.Features.Orders.Commands;
using JewelryShop.Features.Orders.Common;
using JewelryShop.Features.Users.Common;

namespace JewelryShop.Features.Orders.Validators;

/// <summary>
/// Mêmes règles que le devis pour le collier ; le destinataire et l'adresse suivent celles du profil.
/// La présence d'un téléphone et d'une adresse, qui peuvent venir du compte, est vérifiée par le handler.
/// </summary>
public class PlaceOrderCommandValidator : AbstractValidator<PlaceOrderCommand>
{
    public PlaceOrderCommandValidator()
    {
        Include(new NecklaceCompositionValidator<PlaceOrderCommand>());

        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage(NecklaceMessages.InvalidId);

        RuleFor(x => x.Comment)
            .MaximumLength(OrderRules.CommentMaxLength).WithMessage(OrderMessages.CommentTooLong);

        When(x => x.Recipient is not null, () =>
        {
            RuleFor(x => x.Recipient!.Name)
                .NotEmpty().WithMessage(OrderMessages.RecipientNameRequired)
                .MaximumLength(OrderRules.RecipientNameMaxLength).WithMessage(OrderMessages.RecipientNameTooLong);

            RuleFor(x => x.Recipient!.Surname)
                .NotEmpty().WithMessage(OrderMessages.RecipientSurnameRequired)
                .MaximumLength(OrderRules.RecipientNameMaxLength).WithMessage(OrderMessages.RecipientNameTooLong);

            RuleFor(x => x.Recipient!.Phone)
                .Matches(UserAddressRules.PhonePattern).WithMessage(UserMessages.PhoneInvalid)
                .When(x => !string.IsNullOrWhiteSpace(x.Recipient!.Phone));
        });

        When(x => x.Address is not null, () =>
        {
            RuleFor(x => x.Address!.Line1)
                .NotEmpty().WithMessage(UserMessages.AddressLine1Required)
                .MaximumLength(UserAddressRules.AddressLineMaxLength).WithMessage(UserMessages.AddressLineTooLong);

            RuleFor(x => x.Address!.Line2)
                .MaximumLength(UserAddressRules.AddressLineMaxLength).WithMessage(UserMessages.AddressLineTooLong);

            RuleFor(x => x.Address!.PostalCode)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage(UserMessages.PostalCodeInvalid)
                .Matches(UserAddressRules.PostalCodePattern).WithMessage(UserMessages.PostalCodeInvalid);

            RuleFor(x => x.Address!.City)
                .NotEmpty().WithMessage(UserMessages.CityRequired)
                .MaximumLength(UserAddressRules.CityMaxLength).WithMessage(UserMessages.CityTooLong);
        });
    }
}

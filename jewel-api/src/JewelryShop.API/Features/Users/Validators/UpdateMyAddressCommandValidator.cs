using FluentValidation;
using JewelryShop.Features.Users.Commands;
using JewelryShop.Features.Users.Common;

namespace JewelryShop.Features.Users.Validators;

public class UpdateMyAddressCommandValidator : AbstractValidator<UpdateMyAddressCommand>
{
    public UpdateMyAddressCommandValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage(UserMessages.InvalidId);

        RuleFor(x => x.Line1)
            .NotEmpty().WithMessage(UserMessages.AddressLine1Required)
            .MaximumLength(UserAddressRules.AddressLineMaxLength).WithMessage(UserMessages.AddressLineTooLong);

        RuleFor(x => x.Line2)
            .MaximumLength(UserAddressRules.AddressLineMaxLength).WithMessage(UserMessages.AddressLineTooLong);

        RuleFor(x => x.PostalCode)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(UserMessages.PostalCodeInvalid)
            .Matches(UserAddressRules.PostalCodePattern).WithMessage(UserMessages.PostalCodeInvalid);

        RuleFor(x => x.City)
            .NotEmpty().WithMessage(UserMessages.CityRequired)
            .MaximumLength(UserAddressRules.CityMaxLength).WithMessage(UserMessages.CityTooLong);

        RuleFor(x => x.Phone)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(UserMessages.PhoneRequired)
            .Matches(UserAddressRules.PhonePattern).WithMessage(UserMessages.PhoneInvalid);
    }
}

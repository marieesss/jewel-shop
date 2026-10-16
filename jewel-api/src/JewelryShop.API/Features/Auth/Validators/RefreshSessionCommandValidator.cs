using FluentValidation;
using JewelryShop.Features.Auth.Commands;

namespace JewelryShop.Features.Auth.Validators;

/// <summary>
/// N'impose rien : un token absent n'est pas une erreur de saisie mais une session expirée (401),
/// levée par le handler.
/// </summary>
public class RefreshSessionCommandValidator : AbstractValidator<RefreshSessionCommand>
{
}

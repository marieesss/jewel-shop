using FluentValidation;
using JewelryShop.Features.Auth.Commands;

namespace JewelryShop.Features.Auth.Validators;

/// <summary>N'impose rien : se déconnecter sans session active reste un succès.</summary>
public class LogoutCommandValidator : AbstractValidator<LogoutCommand>
{
}

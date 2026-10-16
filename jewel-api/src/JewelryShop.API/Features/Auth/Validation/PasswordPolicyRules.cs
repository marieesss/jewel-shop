using FluentValidation;

namespace JewelryShop.Features.Auth.Validation;

public static class PasswordPolicyRules
{
    public const int MinimumLength = 8;
    public const int MaximumLength = 100;

    public static IRuleBuilderOptions<T, string> PasswordPolicy<T>(this IRuleBuilder<T, string> ruleBuilder) =>
        ruleBuilder
            .NotEmpty().WithMessage("Le mot de passe est requis.")
            .MustWhenNotEmpty(HasMinimumLength)
            .WithMessage($"Le mot de passe doit contenir au moins {MinimumLength} caractères.")
            .MaximumLength(MaximumLength).WithMessage($"{MaximumLength} caractères maximum.")
            .MustWhenNotEmpty(ContainsUppercase)
            .WithMessage("Le mot de passe doit contenir au moins une majuscule.")
            .MustWhenNotEmpty(ContainsDigit)
            .WithMessage("Le mot de passe doit contenir au moins un chiffre.")
            .MustWhenNotEmpty(ContainsSpecialCharacter)
            .WithMessage("Le mot de passe doit contenir au moins un caractère spécial.")
            .MustWhenNotEmpty(ContainsNoWhitespace)
            .WithMessage("Le mot de passe ne doit pas contenir d'espace.");

    // Un mot de passe vide ne doit renvoyer que le message « requis » : FluentValidation
    // ne sait pas arrêter la chaîne après la seule première règle, et When() n'a pas
    // accès à la valeur validée.
    private static IRuleBuilderOptions<T, string> MustWhenNotEmpty<T>(
        this IRuleBuilder<T, string> ruleBuilder,
        Func<string, bool> condition) =>
        ruleBuilder.Must(password => string.IsNullOrEmpty(password) || condition(password));

    private static bool HasMinimumLength(string password) => password.Length >= MinimumLength;

    private static bool ContainsUppercase(string password) => password.Any(char.IsUpper);

    private static bool ContainsDigit(string password) => password.Any(char.IsAsciiDigit);

    private static bool ContainsSpecialCharacter(string password) => password.Any(IsSpecialCharacter);

    private static bool ContainsNoWhitespace(string password) => !password.Any(char.IsWhiteSpace);

    private static bool IsSpecialCharacter(char character) =>
        !char.IsLetter(character) && !char.IsAsciiDigit(character) && !char.IsWhiteSpace(character);
}

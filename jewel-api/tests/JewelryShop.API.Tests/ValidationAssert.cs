using FluentValidation.Results;

namespace JewelryShop.API.Tests;

public static class ValidationAssert
{
    public static void HasError(ValidationResult result, string propertyName, string expectedMessage) =>
        Assert.Contains(
            result.Errors,
            e => e.PropertyName == propertyName && e.ErrorMessage == expectedMessage);

    // Un entier en InlineData désigne une chaîne de cette longueur : un attribut
    // ne peut pas contenir une chaîne construite à l'exécution.
    public static string TextOf(object value) =>
        value is int length ? new string('a', length) : (string)value;
}

using System.Globalization;
using FluentValidation;

namespace JewelryShop.Application.Common;

public static class ValidationCulture
{
    // Sans culture fixée, FluentValidation suit celle du serveur : en production
    // (culture neutre), une règle sans WithMessage répondrait en anglais.
    public static void UseFrench() =>
        ValidatorOptions.Global.LanguageManager.Culture = new CultureInfo("fr");
}

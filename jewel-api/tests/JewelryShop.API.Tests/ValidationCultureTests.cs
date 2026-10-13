using System.Globalization;
using FluentValidation;
using JewelryShop.Application.Common;

namespace JewelryShop.API.Tests;

public sealed class ValidationCultureTests : IDisposable
{
    private readonly CultureInfo? _initialLanguageCulture = ValidatorOptions.Global.LanguageManager.Culture;
    private readonly CultureInfo _initialUiCulture = CultureInfo.CurrentUICulture;

    private sealed record Sample(string Name);

    private sealed class SampleValidatorWithoutMessage : AbstractValidator<Sample>
    {
        public SampleValidatorWithoutMessage() => RuleFor(x => x.Name).NotEmpty();
    }

    [Fact]
    public void Donne_un_message_par_defaut_en_francais_sur_un_serveur_en_culture_neutre()
    {
        ValidatorOptions.Global.LanguageManager.Culture = null;
        CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;

        ValidationCulture.UseFrench();
        var result = new SampleValidatorWithoutMessage().Validate(new Sample(""));

        var error = Assert.Single(result.Errors);
        Assert.Equal("'Name' ne doit pas être vide.", error.ErrorMessage);
    }

    public void Dispose()
    {
        ValidatorOptions.Global.LanguageManager.Culture = _initialLanguageCulture;
        CultureInfo.CurrentUICulture = _initialUiCulture;
    }
}

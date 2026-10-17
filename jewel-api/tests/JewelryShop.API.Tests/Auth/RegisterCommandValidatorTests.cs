using JewelryShop.Features.Auth.Commands;
using JewelryShop.Features.Auth.Validators;

namespace JewelryShop.API.Tests.Auth;

public class RegisterCommandValidatorTests
{
    private readonly RegisterCommandValidator _validator = new();

    private static RegisterCommand ValidCommand() =>
        new("Marie", "Dupont", "marie@mycharmly.eu", "Secret123!", new DateOnly(1995, 4, 12));

    [Fact]
    public void Accepte_une_inscription_valide()
    {
        var result = _validator.Validate(ValidCommand());

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("", "Le prénom est requis.")]
    [InlineData(101, "100 caractères maximum.")]
    public void Refuse_un_prenom_invalide(object name, string expectedMessage)
    {
        var result = _validator.Validate(ValidCommand() with { Name = ValidationAssert.TextOf(name) });

        ValidationAssert.HasError(result, nameof(RegisterCommand.Name), expectedMessage);
    }

    [Theory]
    [InlineData("", "Le nom est requis.")]
    [InlineData(101, "100 caractères maximum.")]
    public void Refuse_un_nom_invalide(object surname, string expectedMessage)
    {
        var result = _validator.Validate(ValidCommand() with { Surname = ValidationAssert.TextOf(surname) });

        ValidationAssert.HasError(result, nameof(RegisterCommand.Surname), expectedMessage);
    }

    [Theory]
    [InlineData("", "L'email est requis.")]
    [InlineData("pas-un-email", "L'email n'est pas valide.")]
    public void Refuse_un_email_invalide(string email, string expectedMessage)
    {
        var result = _validator.Validate(ValidCommand() with { Email = email });

        ValidationAssert.HasError(result, nameof(RegisterCommand.Email), expectedMessage);
    }

    [Fact]
    public void Refuse_un_email_de_plus_de_255_caracteres()
    {
        var email = new string('a', 250) + "@x.com";

        var result = _validator.Validate(ValidCommand() with { Email = email });

        ValidationAssert.HasError(result, nameof(RegisterCommand.Email), "255 caractères maximum.");
    }

    [Theory]
    [InlineData("", "Le mot de passe est requis.")]
    [InlineData("court", "Le mot de passe doit contenir au moins 8 caractères.")]
    [InlineData(101, "100 caractères maximum.")]
    public void Refuse_un_mot_de_passe_invalide(object password, string expectedMessage)
    {
        var result = _validator.Validate(ValidCommand() with { Password = ValidationAssert.TextOf(password) });

        ValidationAssert.HasError(result, nameof(RegisterCommand.Password), expectedMessage);
    }

    [Theory]
    [InlineData("Abcdef1!")]
    [InlineData("Écrin12!")]
    public void Accepte_un_mot_de_passe_conforme_a_la_politique(string password)
    {
        var result = _validator.Validate(ValidCommand() with { Password = password });

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Refuse_un_mot_de_passe_sans_majuscule()
    {
        var result = _validator.Validate(ValidCommand() with { Password = "abcdef1!" });

        ValidationAssert.HasError(
            result,
            nameof(RegisterCommand.Password),
            "Le mot de passe doit contenir au moins une majuscule.");
    }

    [Fact]
    public void Ne_compte_pas_une_minuscule_accentuee_comme_majuscule_ni_comme_caractere_special()
    {
        var result = _validator.Validate(ValidCommand() with { Password = "éabcde1!" });

        Assert.Equal(
            ["Le mot de passe doit contenir au moins une majuscule."],
            result.Errors.Select(e => e.ErrorMessage));
    }

    [Fact]
    public void Refuse_un_mot_de_passe_sans_chiffre()
    {
        var result = _validator.Validate(ValidCommand() with { Password = "Abcdefg!" });

        ValidationAssert.HasError(
            result,
            nameof(RegisterCommand.Password),
            "Le mot de passe doit contenir au moins un chiffre.");
    }

    [Fact]
    public void Refuse_un_mot_de_passe_sans_caractere_special()
    {
        var result = _validator.Validate(ValidCommand() with { Password = "Abcdefg1" });

        ValidationAssert.HasError(
            result,
            nameof(RegisterCommand.Password),
            "Le mot de passe doit contenir au moins un caractère spécial.");
    }

    [Theory]
    [InlineData("Abcd ef1!")]
    [InlineData("Abcd\tef1!")]
    public void Refuse_un_mot_de_passe_contenant_un_espace(string password)
    {
        var result = _validator.Validate(ValidCommand() with { Password = password });

        ValidationAssert.HasError(
            result,
            nameof(RegisterCommand.Password),
            "Le mot de passe ne doit pas contenir d'espace.");
    }

    [Fact]
    public void Renvoie_les_messages_de_toutes_les_regles_non_respectees()
    {
        var result = _validator.Validate(ValidCommand() with { Password = "Ab1 " });

        Assert.Equal(
            [
                "Le mot de passe doit contenir au moins 8 caractères.",
                "Le mot de passe doit contenir au moins un caractère spécial.",
                "Le mot de passe ne doit pas contenir d'espace.",
            ],
            result.Errors.Select(e => e.ErrorMessage));
    }

    [Fact]
    public void Renvoie_seulement_le_message_requis_pour_un_mot_de_passe_vide()
    {
        var result = _validator.Validate(ValidCommand() with { Password = "" });

        Assert.Equal(["Le mot de passe est requis."], result.Errors.Select(e => e.ErrorMessage));
    }

    [Fact]
    public void Refuse_une_date_de_naissance_dans_le_futur()
    {
        var tomorrow = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));

        var result = _validator.Validate(ValidCommand() with { Birthday = tomorrow });

        ValidationAssert.HasError(
            result,
            nameof(RegisterCommand.Birthday),
            "La date de naissance doit être dans le passé.");
    }

    [Fact]
    public void Sans_invitation_refuse_un_email_absent()
    {
        var result = _validator.Validate(ValidCommand() with { Email = null });

        ValidationAssert.HasError(result, nameof(RegisterCommand.Email), "L'email est requis.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("pas-un-email")]
    public void Avec_une_invitation_n_applique_pas_les_regles_de_l_email(string? email)
    {
        var result = _validator.Validate(ValidCommand() with { Email = email, InvitationToken = "token" });

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Refuse_un_token_d_invitation_vide()
    {
        var result = _validator.Validate(ValidCommand() with { InvitationToken = "" });

        ValidationAssert.HasError(
            result,
            nameof(RegisterCommand.InvitationToken),
            "Cette invitation est invalide ou a expiré.");
    }
}

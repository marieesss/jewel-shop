using JewelryShop.Common.Models;
using JewelryShop.Infrastructure.Email;

namespace JewelryShop.API.Tests.Email;

public class PasswordResetEmailTemplateTests
{
    private const string ResetUrl = "http://localhost:5173/reset-password?token=abc_-123";

    private readonly RenderedEmail _email = new ScribanEmailTemplateRenderer()
        .Render(EmailTemplates.PasswordReset, new { FirstName = "Marie", ResetUrl });

    [Fact]
    public void A_pour_sujet_la_reinitialisation_du_mot_de_passe()
    {
        Assert.Equal("Réinitialisation de votre mot de passe mycharmly", _email.Subject);
    }

    [Fact]
    public void Salue_l_utilisateur_et_donne_le_lien_dans_le_texte()
    {
        Assert.Contains("Bonjour Marie,", _email.TextBody);
        Assert.Contains(ResetUrl, _email.TextBody);
        Assert.Contains("valable 1 heure", _email.TextBody);
    }

    [Fact]
    public void Salue_l_utilisateur_et_donne_un_lien_cliquable_dans_le_html()
    {
        Assert.Contains("Bonjour Marie,", _email.HtmlBody);
        Assert.Contains($"<a href=\"{ResetUrl}\">", _email.HtmlBody);
    }

    [Fact]
    public void Echappe_le_prenom_dans_le_html()
    {
        var email = new ScribanEmailTemplateRenderer()
            .Render(EmailTemplates.PasswordReset, new { FirstName = "<b>Marie</b>", ResetUrl });

        Assert.Contains("&lt;b&gt;Marie&lt;/b&gt;", email.HtmlBody);
    }
}

using JewelryShop.Common.Models;
using JewelryShop.Infrastructure.Email;

namespace JewelryShop.API.Tests.Email;

public class InvitationEmailTemplateTests
{
    private const string RegisterUrl = "http://localhost:5173/register?token=abc_-123";

    private readonly RenderedEmail _email = Render("Marie Espinosa");

    [Fact]
    public void A_pour_sujet_l_invitation_a_rejoindre_mycharmly()
    {
        Assert.Equal("Votre invitation à rejoindre mycharmly", _email.Subject);
    }

    [Fact]
    public void Nomme_l_admin_et_donne_le_lien_et_la_date_d_expiration_dans_le_texte()
    {
        Assert.StartsWith("Bonjour,", _email.TextBody);
        Assert.Contains("Marie Espinosa vous invite à créer votre compte sur mycharmly.", _email.TextBody);
        Assert.Contains("Il est valable jusqu'au 22/10/2026.", _email.TextBody);
        Assert.Contains(RegisterUrl, _email.TextBody);
        Assert.Contains("Si vous ne vous attendiez pas à cette invitation, ignorez ce mail.", _email.TextBody);
    }

    [Fact]
    public void Nomme_l_admin_et_donne_un_lien_cliquable_dans_le_html()
    {
        Assert.Contains("Marie Espinosa vous invite à créer votre compte sur mycharmly.", _email.HtmlBody);
        Assert.Contains($"<a href=\"{RegisterUrl}\">", _email.HtmlBody);
    }

    [Fact]
    public void Utilise_une_tournure_impersonnelle_si_l_admin_n_a_pas_de_nom()
    {
        var email = Render(null);

        Assert.Contains("Bonjour,", email.TextBody);
        Assert.Contains("Vous êtes invité à créer votre compte sur mycharmly. Pour choisir", email.TextBody);
        Assert.Contains("Vous êtes invité à créer votre compte sur mycharmly. Pour choisir", email.HtmlBody);
    }

    [Fact]
    public void Echappe_le_nom_de_l_admin_dans_le_html()
    {
        var email = Render("<b>Marie</b>");

        Assert.Contains("&lt;b&gt;Marie&lt;/b&gt;", email.HtmlBody);
    }

    private static RenderedEmail Render(string? inviterName) => new ScribanEmailTemplateRenderer()
        .Render(EmailTemplates.Invitation, new { InviterName = inviterName, RegisterUrl, ExpiresAt = "22/10/2026" });
}

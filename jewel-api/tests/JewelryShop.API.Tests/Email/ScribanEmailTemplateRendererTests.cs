using JewelryShop.Infrastructure.Email;

namespace JewelryShop.API.Tests.Email;

public class ScribanEmailTemplateRendererTests
{
    private readonly ScribanEmailTemplateRenderer _renderer = new();

    [Fact]
    public void Rend_le_sujet_le_html_et_le_texte_avec_les_valeurs_du_modele()
    {
        var email = _renderer.Render(EmailTemplates.Test, new { FirstName = "Marie" });

        Assert.Contains("Marie", email.Subject);
        Assert.Contains("Marie", email.HtmlBody);
        Assert.Contains("Marie", email.TextBody);
    }

    [Fact]
    public void Echappe_les_valeurs_dans_le_html()
    {
        var email = _renderer.Render(EmailTemplates.Test, new { FirstName = "<script>" });

        Assert.Contains("&lt;script&gt;", email.HtmlBody);
        Assert.DoesNotContain("<script>", email.HtmlBody);
    }

    [Fact]
    public void N_echappe_pas_les_valeurs_dans_le_texte()
    {
        var email = _renderer.Render(EmailTemplates.Test, new { FirstName = "<script>" });

        Assert.Contains("<script>", email.TextBody);
    }

    [Fact]
    public void Leve_une_erreur_mentionnant_le_nom_pour_un_template_inconnu()
    {
        var exception = Assert.Throws<InvalidOperationException>(
            () => _renderer.Render("Inexistant", new { FirstName = "Marie" }));

        Assert.Contains("Inexistant", exception.Message);
    }

    [Fact]
    public void Leve_une_erreur_mentionnant_le_nom_pour_un_template_mal_ecrit()
    {
        var renderer = RendererWith(subject: "Bonjour {{ end }}", html: "", text: "");

        var exception = Assert.Throws<InvalidOperationException>(
            () => renderer.Render("Casse", new { FirstName = "Marie" }));

        Assert.Contains("Casse", exception.Message);
    }

    [Fact]
    public void Leve_une_erreur_quand_une_variable_manque_dans_le_modele()
    {
        var exception = Assert.Throws<InvalidOperationException>(
            () => _renderer.Render(EmailTemplates.Test, new { LastName = "Dupont" }));

        Assert.Contains(EmailTemplates.Test, exception.Message);
    }

    [Theory]
    [InlineData("  Bonjour {{ first_name }}  \n")]
    [InlineData("Bonjour\r\n{{ first_name }}")]
    public void Rend_le_sujet_sur_une_seule_ligne_sans_espaces_autour(string subject)
    {
        var renderer = RendererWith(subject, html: "", text: "");

        var email = renderer.Render("Sujet", new { FirstName = "Marie" });

        Assert.Equal("Bonjour Marie", email.Subject);
    }

    [Fact]
    public void Donne_le_meme_resultat_a_chaque_rendu_d_un_template_en_cache()
    {
        var first = _renderer.Render(EmailTemplates.Test, new { FirstName = "Marie" });
        var second = _renderer.Render(EmailTemplates.Test, new { FirstName = "Marie" });

        Assert.Equal(first, second);
    }

    [Fact]
    public void Ne_lit_les_fichiers_d_un_template_qu_une_seule_fois()
    {
        var reads = 0;
        var renderer = new ScribanEmailTemplateRenderer(_ =>
        {
            reads++;
            return "Bonjour {{ first_name }}";
        });

        renderer.Render("Compte", new { FirstName = "Marie" });
        renderer.Render("Compte", new { FirstName = "Paul" });

        Assert.Equal(3, reads);
    }

    private static ScribanEmailTemplateRenderer RendererWith(string subject, string html, string text) =>
        new(fileName => fileName switch
        {
            _ when fileName.EndsWith(".subject.sbn") => subject,
            _ when fileName.EndsWith(".html.sbn") => html,
            _ when fileName.EndsWith(".txt.sbn") => text,
            _ => null,
        });
}

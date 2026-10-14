using JewelryShop.Infrastructure.Email;

namespace JewelryShop.API.Tests.Email;

public class EmailOptionsTests
{
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void N_est_pas_configure_quand_l_hote_est_vide(string host)
    {
        var options = new EmailOptions { Smtp = { Host = host } };

        Assert.False(options.IsConfigured);
    }

    [Fact]
    public void Est_configure_quand_l_hote_est_renseigne()
    {
        var options = new EmailOptions { Smtp = { Host = "mailpit" } };

        Assert.True(options.IsConfigured);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Ne_demande_pas_d_authentification_quand_l_identifiant_est_vide(string username)
    {
        var options = new SmtpOptions { Username = username };

        Assert.False(options.RequiresAuthentication);
    }

    [Fact]
    public void Demande_une_authentification_quand_l_identifiant_est_renseigne()
    {
        var options = new SmtpOptions { Username = "user" };

        Assert.True(options.RequiresAuthentication);
    }

    [Fact]
    public void Applique_les_valeurs_par_defaut_de_la_specification()
    {
        var options = new EmailOptions();

        Assert.Equal(1025, options.Smtp.Port);
        Assert.False(options.Smtp.UseSsl);
        Assert.Equal("no-reply@mycharmly.eu", options.FromAddress);
        Assert.Equal("mycharmly", options.FromName);
    }
}

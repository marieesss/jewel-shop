using JewelryShop.Common;

namespace JewelryShop.API.Tests;

public class AppOptionsTests
{
    [Fact]
    public void Pointe_par_defaut_vers_le_front_de_developpement()
    {
        Assert.Equal("http://localhost:5173", new AppOptions().FrontendBaseUrl);
    }

    [Theory]
    [InlineData("https://mycharmly.eu")]
    [InlineData("https://mycharmly.eu/")]
    public void Construit_un_lien_sans_double_barre_oblique(string baseUrl)
    {
        var options = new AppOptions { FrontendBaseUrl = baseUrl };

        Assert.Equal("https://mycharmly.eu/reset-password?token=abc", options.FrontendUrl("/reset-password?token=abc"));
    }
}

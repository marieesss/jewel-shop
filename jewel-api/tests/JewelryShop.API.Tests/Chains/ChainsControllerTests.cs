using System.Security.Claims;
using JewelryShop.API.Tests.Fakes;
using JewelryShop.Common.Enums;
using JewelryShop.Features.Chains.Commands;
using JewelryShop.Features.Chains.Controllers;
using JewelryShop.Features.Chains.Queries;
using Microsoft.AspNetCore.Http;

namespace JewelryShop.API.Tests.Chains;

public class ChainsControllerTests
{
    private readonly FakeSender _sender = new();
    private readonly ChainsController _controller = new();

    public ChainsControllerTests() => _sender.AttachTo(_controller, userId: 42);

    [Fact]
    public async Task Demande_les_favoris_de_l_utilisateur_connecte_en_premier()
    {
        await _controller.GetAll(null, null, null, favoritesFirst: true);

        var sent = Assert.IsType<GetChainsQuery>(_sender.LastRequest);
        Assert.Equal(42, sent.FavoritesFirstForUserId);
    }

    [Fact]
    public async Task Ignore_les_favoris_en_premier_pour_un_appel_anonyme()
    {
        _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity());

        await _controller.GetAll(null, null, null, favoritesFirst: true);

        var sent = Assert.IsType<GetChainsQuery>(_sender.LastRequest);
        Assert.Null(sent.FavoritesFirstForUserId);
    }

    [Fact]
    public async Task Ne_demande_pas_les_favoris_en_premier_sans_le_parametre()
    {
        await _controller.GetAll(null, null, null);

        var sent = Assert.IsType<GetChainsQuery>(_sender.LastRequest);
        Assert.Null(sent.FavoritesFirstForUserId);
    }

    [Fact]
    public async Task Met_a_jour_la_chaine_de_la_route_et_non_celle_du_corps()
    {
        var body = new UpdateChainCommand(99, "Forçat", "", ProductColor.Gold, 5, 20, 45);

        await _controller.Update(7, body, CancellationToken.None);

        var sent = Assert.IsType<UpdateChainCommand>(_sender.LastRequest);
        Assert.Equal(body with { Id = 7 }, sent);
    }

    [Fact]
    public async Task Envoie_une_commande_d_upload_vide_quand_aucun_fichier_n_est_fourni()
    {
        _sender.Response = "";

        await _controller.UploadImage(7, null, CancellationToken.None);

        var sent = Assert.IsType<UploadChainImageCommand>(_sender.LastRequest);
        Assert.Equal(new UploadChainImageCommand(7, Stream.Null, 0, string.Empty, string.Empty), sent);
    }

    [Fact]
    public async Task Envoie_le_fichier_recu_dans_la_commande_d_upload()
    {
        _sender.Response = "https://img.mycharmly.eu/photo.jpg";
        var file = new FormFile(new MemoryStream([1, 2, 3]), 0, 3, "file", "photo.jpg")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/jpeg",
        };

        await _controller.UploadImage(7, file, CancellationToken.None);

        var sent = Assert.IsType<UploadChainImageCommand>(_sender.LastRequest);
        Assert.Equal(7, sent.ChainId);
        Assert.Equal(3, sent.Length);
        Assert.Equal("photo.jpg", sent.FileName);
        Assert.Equal("image/jpeg", sent.ContentType);
    }
}

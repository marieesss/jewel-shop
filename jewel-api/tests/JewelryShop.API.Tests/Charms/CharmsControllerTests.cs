using JewelryShop.API.Tests.Fakes;
using JewelryShop.Common.Enums;
using JewelryShop.Features.Charms.Commands;
using JewelryShop.Features.Charms.Controllers;
using Microsoft.AspNetCore.Http;

namespace JewelryShop.API.Tests.Charms;

public class CharmsControllerTests
{
    private readonly FakeSender _sender = new();
    private readonly CharmsController _controller = new();

    public CharmsControllerTests() => _sender.AttachTo(_controller);

    [Fact]
    public async Task Met_a_jour_la_breloque_de_la_route_et_non_celle_du_corps()
    {
        var body = new UpdateCharmCommand(99, "Trèfle", "", ProductColor.Silver, 2, 8, 10);

        await _controller.Update(7, body, CancellationToken.None);

        var sent = Assert.IsType<UpdateCharmCommand>(_sender.LastRequest);
        Assert.Equal(body with { Id = 7 }, sent);
    }

    [Fact]
    public async Task Envoie_une_commande_d_upload_vide_quand_aucun_fichier_n_est_fourni()
    {
        _sender.Response = "";

        await _controller.UploadImage(7, null, CancellationToken.None);

        var sent = Assert.IsType<UploadCharmImageCommand>(_sender.LastRequest);
        Assert.Equal(new UploadCharmImageCommand(7, Stream.Null, 0, string.Empty, string.Empty), sent);
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

        var sent = Assert.IsType<UploadCharmImageCommand>(_sender.LastRequest);
        Assert.Equal(7, sent.CharmId);
        Assert.Equal(3, sent.Length);
        Assert.Equal("photo.jpg", sent.FileName);
        Assert.Equal("image/jpeg", sent.ContentType);
    }
}

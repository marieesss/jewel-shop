using JewelryShop.API.Tests.Fakes;
using JewelryShop.Features.Auth.Commands;
using JewelryShop.Features.Auth.Controllers;
using JewelryShop.Features.Invitations.DTOs;
using JewelryShop.Features.Invitations.Queries;
using Microsoft.AspNetCore.Mvc;

namespace JewelryShop.API.Tests.Auth;

public class AuthControllerTests
{
    private readonly FakeSender _sender = new();
    private readonly AuthController _controller = new();

    public AuthControllerTests() => _sender.AttachTo(_controller);

    [Fact]
    public async Task Forgot_password_envoie_la_commande_et_repond_202_sans_corps()
    {
        var command = new ForgotPasswordCommand("marie@mycharmly.eu");

        var result = await _controller.ForgotPassword(command, CancellationToken.None);

        Assert.Equal(command, _sender.LastRequest);
        var accepted = Assert.IsType<AcceptedResult>(result);
        Assert.Null(accepted.Value);
    }

    [Fact]
    public async Task Reset_password_envoie_la_commande_et_repond_204()
    {
        var command = new ResetPasswordCommand("token", "Nouveau123!");

        var result = await _controller.ResetPassword(command, CancellationToken.None);

        Assert.Equal(command, _sender.LastRequest);
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Get_invitation_envoie_la_requete_avec_le_token_de_la_route_et_repond_200()
    {
        var preview = new InvitationPreviewDto("nouvelle@exemple.com", new DateTime(2026, 10, 22, 8, 0, 0, DateTimeKind.Utc));
        _sender.Response = preview;

        var result = await _controller.GetInvitation("token", CancellationToken.None);

        Assert.Equal(new GetInvitationByTokenQuery("token"), _sender.LastRequest);
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(preview, ok.Value);
    }
}

using JewelryShop.API.Tests.Fakes;
using JewelryShop.Features.Auth.Commands;
using JewelryShop.Features.Auth.Controllers;
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
}

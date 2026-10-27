using System.Reflection;
using JewelryShop.API.Tests.Fakes;
using JewelryShop.Features.Users.Commands;
using JewelryShop.Features.Users.Controllers;
using JewelryShop.Features.Users.DTOs;
using JewelryShop.Features.Users.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JewelryShop.API.Tests.Users;

public class UsersControllerTests
{
    private const long ConnectedUserId = 4;

    private readonly FakeSender _sender = new();
    private readonly UsersController _controller = new();

    public UsersControllerTests() => _sender.AttachTo(_controller, ConnectedUserId);

    private static ProfileDto Profile() =>
        new(ConnectedUserId, "Marie", "Durand", "marie@example.com", null, null, null);

    [Fact]
    public async Task Renvoie_le_profil_de_l_utilisateur_connecte()
    {
        var profile = Profile();
        _sender.Response = profile;

        var response = await _controller.GetMe(CancellationToken.None);

        var sent = Assert.IsType<GetMyProfileQuery>(_sender.LastRequest);
        Assert.Equal(ConnectedUserId, sent.UserId);
        Assert.Same(profile, Assert.IsType<OkObjectResult>(response).Value);
    }

    [Fact]
    public async Task Enregistre_l_adresse_de_l_utilisateur_connecte_et_non_celui_du_corps()
    {
        var profile = Profile();
        _sender.Response = profile;
        var body = new UpdateMyAddressCommand(99, "12 rue des Lilas", null, "75011", "Paris", "0612345678");

        var response = await _controller.UpdateMyAddress(body, CancellationToken.None);

        var sent = Assert.IsType<UpdateMyAddressCommand>(_sender.LastRequest);
        Assert.Equal(body with { UserId = ConnectedUserId }, sent);
        Assert.Same(profile, Assert.IsType<OkObjectResult>(response).Value);
    }

    [Fact]
    public void Est_reserve_aux_utilisateurs_connectes_sous_la_route_api_users()
    {
        var authorize = typeof(UsersController).GetCustomAttribute<AuthorizeAttribute>();
        var route = typeof(UsersController).GetCustomAttribute<RouteAttribute>();

        Assert.NotNull(authorize);
        Assert.Null(authorize.Roles);
        Assert.Equal("api/users", route?.Template);
    }
}

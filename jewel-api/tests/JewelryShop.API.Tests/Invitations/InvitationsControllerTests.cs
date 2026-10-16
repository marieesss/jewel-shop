using System.Reflection;
using JewelryShop.API.Tests.Fakes;
using JewelryShop.Features.Invitations.Commands;
using JewelryShop.Features.Invitations.Controllers;
using JewelryShop.Features.Invitations.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace JewelryShop.API.Tests.Invitations;

public class InvitationsControllerTests
{
    private const long AdminId = 7;

    private readonly FakeSender _sender = new();
    private readonly InvitationsController _controller = new();

    public InvitationsControllerTests() => _sender.AttachTo(_controller, AdminId);

    [Fact]
    public async Task Envoie_la_commande_avec_l_admin_connecte_et_non_celui_du_corps()
    {
        var body = new CreateInvitationCommand("nouvelle@exemple.com", 99);

        await _controller.Create(body, CancellationToken.None);

        var sent = Assert.IsType<CreateInvitationCommand>(_sender.LastRequest);
        Assert.Equal(body with { InvitedById = AdminId }, sent);
    }

    [Fact]
    public async Task Repond_201_avec_l_invitation_creee()
    {
        var invitation = new InvitationDto(1, "nouvelle@exemple.com", "Pending", DateTime.UtcNow.AddDays(7), DateTime.UtcNow, "Marie Espinosa");
        _sender.Response = invitation;

        var response = await _controller.Create(new CreateInvitationCommand("nouvelle@exemple.com", 0), CancellationToken.None);

        var result = Assert.IsType<ObjectResult>(response);
        Assert.Equal(StatusCodes.Status201Created, result.StatusCode);
        Assert.Same(invitation, result.Value);
    }

    [Fact]
    public void Est_reserve_aux_admins_sous_la_route_api_admin_invitations()
    {
        var authorize = typeof(InvitationsController).GetCustomAttribute<AuthorizeAttribute>();
        var route = typeof(InvitationsController).GetCustomAttribute<RouteAttribute>();

        Assert.Equal("Admin", authorize?.Roles);
        Assert.Equal("api/admin/invitations", route?.Template);
        Assert.Null(typeof(InvitationsController).GetMethod(nameof(InvitationsController.Create))!
            .GetCustomAttribute<AllowAnonymousAttribute>());
    }
}

using System.Reflection;
using JewelryShop.API.Tests.Fakes;
using JewelryShop.Common;
using JewelryShop.Features.Orders.Commands;
using JewelryShop.Features.Orders.Controllers;
using JewelryShop.Features.Orders.DTOs;
using JewelryShop.Features.Orders.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JewelryShop.API.Tests.Orders;

public class AdminOrdersControllerTests
{
    private const long ConnectedAdminId = 1;

    private static readonly DateTime _now = new(2026, 11, 17, 9, 0, 0, DateTimeKind.Utc);

    private readonly FakeSender _sender = new();
    private readonly AdminOrdersController _controller = new();

    public AdminOrdersControllerTests() => _sender.AttachTo(_controller, ConnectedAdminId);

    private static OrderNoteDto Note() =>
        new(7, "Emballage cadeau.", new OrderUserDto(ConnectedAdminId, "Alice", "Martin"), _now, _now);

    [Fact]
    public void Est_reserve_aux_admins_sous_la_route_api_admin_orders()
    {
        var authorize = typeof(AdminOrdersController).GetCustomAttribute<AuthorizeAttribute>();
        var route = typeof(AdminOrdersController).GetCustomAttribute<RouteAttribute>();

        Assert.Equal("Admin", authorize?.Roles);
        Assert.Equal("api/admin/orders", route?.Template);
    }

    [Fact]
    public async Task Liste_les_commandes_avec_le_statut_et_la_pagination_demandes()
    {
        var page = new PagedResult<AdminOrderSummaryDto>([], 0, 2, 10);
        _sender.Response = page;

        var response = await _controller.GetAll("Shipped", 2, 10, CancellationToken.None);

        Assert.Equal(new GetOrdersQuery("Shipped", 2, 10), _sender.LastRequest);
        Assert.Same(page, Assert.IsType<OkObjectResult>(response).Value);
    }

    [Fact]
    public async Task Consulte_le_detail_d_une_commande()
    {
        await _controller.GetById(42, CancellationToken.None);

        Assert.Equal(new GetOrderByIdQuery(42), _sender.LastRequest);
    }

    [Fact]
    public async Task Ajoute_la_note_au_nom_de_l_admin_connecte_et_repond_201_avec_son_url()
    {
        var note = Note();
        _sender.Response = note;

        var response = await _controller.AddNote(42, new AddOrderNoteCommand(99, 99, "Emballage cadeau."), CancellationToken.None);

        Assert.Equal(new AddOrderNoteCommand(42, ConnectedAdminId, "Emballage cadeau."), _sender.LastRequest);
        var created = Assert.IsType<CreatedResult>(response);
        Assert.Equal("/api/admin/orders/42/notes/7", created.Location);
        Assert.Same(note, created.Value);
    }

    [Fact]
    public async Task Modifie_la_note_de_la_commande_designee_par_l_url()
    {
        var note = Note();
        _sender.Response = note;

        var response = await _controller.UpdateNote(42, 7, new UpdateOrderNoteCommand(99, 99, "Ruban rouge."), CancellationToken.None);

        Assert.Equal(new UpdateOrderNoteCommand(42, 7, "Ruban rouge."), _sender.LastRequest);
        Assert.Same(note, Assert.IsType<OkObjectResult>(response).Value);
    }
}

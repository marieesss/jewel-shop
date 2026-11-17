using System.Net;
using System.Net.Http.Json;
using JewelryShop.API.IntegrationTests.Chains;
using JewelryShop.API.IntegrationTests.Infrastructure;
using JewelryShop.API.IntegrationTests.Users;
using JewelryShop.Common;
using JewelryShop.Common.Enums;
using JewelryShop.Features.Orders.DTOs;
using JewelryShop.Features.Orders.Entities;

namespace JewelryShop.API.IntegrationTests.Orders;

/// <summary>Commandes vues par l'admin : liste de tous les clients, détail et notes internes.</summary>
[Collection(ApiCollection.Name)]
public class AdminOrdersTests
{
    private const string AdminPath = "/api/admin/orders";

    private readonly JewelryShopApiFactory _api;

    public AdminOrdersTests(JewelryShopApiFactory api) => _api = api;

    private async Task<(long CustomerId, long OrderId)> SeedCustomerOrderAsync(OrderStatus status = OrderStatus.Pending)
    {
        var customer = await _api.CreateUserAsync();
        var orderId = await _api.SeedOrderForAsync(customer.Id, status, DateTime.UtcNow);
        return (customer.Id, orderId);
    }

    [Fact]
    public async Task Filtre_la_liste_par_statut_et_donne_le_client_de_chaque_commande()
    {
        var admin = await _api.CreateClientAsAsync(UserRole.Admin);
        var customer = await _api.CreateUserAsync();
        // Dates hors de portée des autres tests : la base est partagée.
        var start = new DateTime(2999, 1, 1, 8, 0, 0, DateTimeKind.Utc);
        var older = await _api.SeedOrderForAsync(customer.Id, OrderStatus.Shipped, start);
        await _api.SeedOrderForAsync(customer.Id, OrderStatus.Pending, start.AddDays(1));
        var newer = await _api.SeedOrderForAsync(customer.Id, OrderStatus.Shipped, start.AddDays(2));

        var response = await admin.GetAsync($"{AdminPath}?status=Shipped&pageSize=100");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var page = await response.Content.ReadFromJsonAsync<PagedResult<AdminOrderSummaryDto>>();
        Assert.NotNull(page);
        Assert.All(page.Items, o => Assert.Equal("Shipped", o.Status));
        Assert.Equal([newer, older], page.Items.Take(2).Select(o => o.Id));
        Assert.Equal((customer.Id, customer.Name, customer.Surname), (page.Items[0].Customer.Id, page.Items[0].Customer.Name, page.Items[0].Customer.Surname));
    }

    [Fact]
    public async Task Refuse_un_statut_inconnu_avec_400()
    {
        var admin = await _api.CreateClientAsAsync(UserRole.Admin);

        var response = await admin.GetAsync($"{AdminPath}?status=Perdue");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ApiError>();
        Assert.NotNull(error?.Errors);
        Assert.Contains("Le statut n'est pas valide.", Assert.Contains("Status", error.Errors));
    }

    [Fact]
    public async Task Montre_dans_le_detail_la_note_ajoutee_avec_son_auteur_et_sa_date()
    {
        var admin = await _api.CreateClientAsAsync(UserRole.Admin);
        var adminProfile = await admin.GetMyProfileAsync();
        var (customerId, orderId) = await SeedCustomerOrderAsync();

        var added = await admin.PostAsJsonAsync($"{AdminPath}/{orderId}/notes", new { Content = "Emballage cadeau." });

        Assert.Equal(HttpStatusCode.Created, added.StatusCode);
        var note = await added.Content.ReadFromJsonAsync<OrderNoteDto>();
        Assert.NotNull(note);
        Assert.Equal($"{AdminPath}/{orderId}/notes/{note.Id}", added.Headers.Location?.OriginalString);

        var detail = await admin.GetFromJsonAsync<AdminOrderDto>($"{AdminPath}/{orderId}");
        Assert.NotNull(detail);
        Assert.Equal(customerId, detail.Customer.Id);
        var shown = Assert.Single(detail.Notes);
        Assert.Equal((note.Id, "Emballage cadeau."), (shown.Id, shown.Content));
        Assert.Equal((adminProfile.Id, adminProfile.Name), (shown.Author.Id, shown.Author.Name));
        Assert.Equal(note.CreatedAt, shown.CreatedAt);
    }

    [Fact]
    public async Task Modifie_le_contenu_et_la_date_de_modification_d_une_note()
    {
        var admin = await _api.CreateClientAsAsync(UserRole.Admin);
        var (_, orderId) = await SeedCustomerOrderAsync();
        var created = await (await admin.PostAsJsonAsync($"{AdminPath}/{orderId}/notes", new { Content = "Emballage cadeau." }))
            .Content.ReadFromJsonAsync<OrderNoteDto>();

        var response = await admin.PutAsJsonAsync($"{AdminPath}/{orderId}/notes/{created!.Id}", new { Content = "Ruban rouge." });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await response.Content.ReadFromJsonAsync<OrderNoteDto>();
        Assert.Equal("Ruban rouge.", updated?.Content);
        Assert.Equal(created.CreatedAt, updated?.CreatedAt);
        Assert.True(updated?.UpdatedAt > created.UpdatedAt);
        var detail = await admin.GetFromJsonAsync<AdminOrderDto>($"{AdminPath}/{orderId}");
        Assert.Equal("Ruban rouge.", Assert.Single(detail!.Notes).Content);
    }

    [Fact]
    public async Task Refuse_une_note_vide_avec_400()
    {
        var admin = await _api.CreateClientAsAsync(UserRole.Admin);
        var (_, orderId) = await SeedCustomerOrderAsync();

        var response = await admin.PostAsJsonAsync($"{AdminPath}/{orderId}/notes", new { Content = "  " });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ApiError>();
        Assert.NotNull(error?.Errors);
        Assert.Contains("La note est requise.", Assert.Contains("Content", error.Errors));
    }

    [Fact]
    public async Task Repond_404_pour_une_commande_inconnue()
    {
        var admin = await _api.CreateClientAsAsync(UserRole.Admin);

        var response = await admin.GetAsync($"{AdminPath}/{long.MaxValue}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("Commande introuvable.", (await response.Content.ReadFromJsonAsync<ApiError>())?.Title);
    }

    [Fact]
    public async Task Repond_404_pour_la_note_d_une_autre_commande()
    {
        var admin = await _api.CreateClientAsAsync(UserRole.Admin);
        var (_, orderId) = await SeedCustomerOrderAsync();
        var (_, otherOrderId) = await SeedCustomerOrderAsync();
        var note = await (await admin.PostAsJsonAsync($"{AdminPath}/{orderId}/notes", new { Content = "Emballage cadeau." }))
            .Content.ReadFromJsonAsync<OrderNoteDto>();

        var response = await admin.PutAsJsonAsync($"{AdminPath}/{otherOrderId}/notes/{note!.Id}", new { Content = "Ruban rouge." });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("Note introuvable.", (await response.Content.ReadFromJsonAsync<ApiError>())?.Title);
    }

    [Fact]
    public async Task Refuse_les_endpoints_admin_a_un_utilisateur_non_admin_avec_403()
    {
        var user = await _api.CreateClientAsAsync(UserRole.User);
        var (_, orderId) = await SeedCustomerOrderAsync();

        var list = await user.GetAsync(AdminPath);
        var detail = await user.GetAsync($"{AdminPath}/{orderId}");
        var note = await user.PostAsJsonAsync($"{AdminPath}/{orderId}/notes", new { Content = "Note" });
        var update = await user.PutAsJsonAsync($"{AdminPath}/{orderId}/notes/1", new { Content = "Note" });

        Assert.Equal(
            [HttpStatusCode.Forbidden, HttpStatusCode.Forbidden, HttpStatusCode.Forbidden, HttpStatusCode.Forbidden],
            [list.StatusCode, detail.StatusCode, note.StatusCode, update.StatusCode]);
    }
}

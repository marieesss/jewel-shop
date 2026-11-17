using System.Net;
using System.Net.Http.Json;
using JewelryShop.API.IntegrationTests.Chains;
using JewelryShop.API.IntegrationTests.Infrastructure;
using JewelryShop.API.IntegrationTests.Users;
using JewelryShop.Common;
using JewelryShop.Common.Enums;
using JewelryShop.Features.Orders.DTOs;
using JewelryShop.Features.Orders.Entities;
using JewelryShop.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace JewelryShop.API.IntegrationTests.Orders;

/// <summary>Suivi de ses commandes par le client : liste, détail, annulation et réception.</summary>
[Collection(ApiCollection.Name)]
public class CustomerOrdersTests
{
    private const int RollLengthCm = 250;

    private readonly JewelryShopApiFactory _api;

    public CustomerOrdersTests(JewelryShopApiFactory api) => _api = api;

    [Fact]
    public async Task Liste_les_20_commandes_les_plus_recentes_sur_25()
    {
        var customer = await _api.CreateClientAsAsync(UserRole.User);
        var userId = (await customer.GetMyProfileAsync()).Id;
        var start = new DateTime(2026, 1, 1, 8, 0, 0, DateTimeKind.Utc);
        var ids = new List<long>();
        for (var day = 0; day < 25; day++)
            ids.Add(await _api.SeedOrderForAsync(userId, OrderStatus.Pending, start.AddDays(day)));
        await _api.SeedOrderForAsync((await _api.CreateUserAsync()).Id, OrderStatus.Pending, start.AddDays(30));

        var response = await customer.GetAsync(OrdersApi.Path);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var page = await response.Content.ReadFromJsonAsync<PagedResult<OrderSummaryDto>>();
        Assert.NotNull(page);
        Assert.Equal(25, page.TotalCount);
        Assert.Equal(Enumerable.Reverse(ids).Take(20), page.Items.Select(o => o.Id));
        Assert.Equal("Pending", page.Items[0].Status);
        Assert.Equal(start.AddDays(24), page.Items[0].CreatedAt);
    }

    [Fact]
    public async Task Donne_le_detail_de_sa_commande()
    {
        var (customer, order, _, _) = await PlaceOrderAsync();

        var response = await customer.GetAsync($"{OrdersApi.Path}/{order.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var detail = await response.Content.ReadFromJsonAsync<OrderDto>();
        Assert.Equal(order.Number, detail?.Number);
        Assert.Equal(order.Lines, detail?.Lines);
        Assert.Equal("Pending", Assert.Single(detail!.StatusHistory).ToStatus);
    }

    [Fact]
    public async Task Repond_404_pour_la_commande_d_un_autre_utilisateur()
    {
        var (_, order, _, _) = await PlaceOrderAsync();
        var stranger = await _api.CreateCustomerClientAsync();

        var detail = await stranger.GetAsync($"{OrdersApi.Path}/{order.Id}");
        var cancel = await stranger.PostAsync($"{OrdersApi.Path}/{order.Id}/cancel", null);

        Assert.Equal(HttpStatusCode.NotFound, detail.StatusCode);
        Assert.Equal("Commande introuvable.", (await detail.Content.ReadFromJsonAsync<ApiError>())?.Title);
        Assert.Equal(HttpStatusCode.NotFound, cancel.StatusCode);
        Assert.Equal("Pending", await StatusOfAsync(order.Id));
    }

    [Fact]
    public async Task Annule_une_commande_en_validation_recredite_le_stock_et_previent_les_admins()
    {
        var (customer, order, chainId, charmId) = await PlaceOrderAsync();
        Assert.Equal(1, await customer.GetCharmStockAsync(charmId));

        var response = await customer.PostAsync($"{OrdersApi.Path}/{order.Id}/cancel", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var cancelled = await response.Content.ReadFromJsonAsync<OrderDto>();
        Assert.Equal("Cancelled", cancelled?.Status);
        Assert.Equal(("Pending", "Cancelled"), (cancelled!.StatusHistory[^1].FromStatus, cancelled.StatusHistory[^1].ToStatus));
        Assert.Equal(3, await customer.GetCharmStockAsync(charmId));
        Assert.Equal([RollLengthCm], await RemainingRollLengthsAsync(chainId));
        var (adminEmails, mails) = await AdminMailsAsync($"Commande {order.Number} annulée par");
        Assert.Equal(adminEmails.Order(), mails.Order());
    }

    [Fact]
    public async Task Refuse_avec_409_d_annuler_une_commande_en_preparation_sans_rien_modifier()
    {
        var (customer, order, chainId, charmId) = await PlaceOrderAsync();
        await _api.SetOrderStatusAsync(order.Id, OrderStatus.Processing);

        var response = await customer.PostAsync($"{OrdersApi.Path}/{order.Id}/cancel", null);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal(
            "Cette commande ne peut plus être annulée.",
            (await response.Content.ReadFromJsonAsync<ApiError>())?.Title);
        Assert.Equal("Processing", await StatusOfAsync(order.Id));
        Assert.Equal(1, await customer.GetCharmStockAsync(charmId));
        Assert.Equal([RollLengthCm - OrdersApi.OfferedLengthCm], await RemainingRollLengthsAsync(chainId));
        Assert.Empty((await AdminMailsAsync($"Commande {order.Number} annulée par")).Mails);
    }

    [Fact]
    public async Task Marque_recue_une_commande_envoyee_et_previent_les_admins()
    {
        var (customer, order, _, charmId) = await PlaceOrderAsync();
        await _api.SetOrderStatusAsync(order.Id, OrderStatus.Shipped);

        var response = await customer.PostAsync($"{OrdersApi.Path}/{order.Id}/receive", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var received = await response.Content.ReadFromJsonAsync<OrderDto>();
        Assert.Equal("Received", received?.Status);
        Assert.Equal(("Shipped", "Received"), (received!.StatusHistory[^1].FromStatus, received.StatusHistory[^1].ToStatus));
        Assert.Equal(1, await customer.GetCharmStockAsync(charmId));
        var (adminEmails, mails) = await AdminMailsAsync($"Commande {order.Number} reçue par");
        Assert.Equal(adminEmails.Order(), mails.Order());
    }

    [Fact]
    public async Task Refuse_avec_409_de_marquer_recue_une_commande_pas_encore_envoyee()
    {
        var (customer, order, _, _) = await PlaceOrderAsync();

        var response = await customer.PostAsync($"{OrdersApi.Path}/{order.Id}/receive", null);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal(
            "Cette commande ne peut pas être marquée comme reçue.",
            (await response.Content.ReadFromJsonAsync<ApiError>())?.Title);
        Assert.Equal("Pending", await StatusOfAsync(order.Id));
    }

    [Fact]
    public async Task Refuse_un_visiteur_anonyme_avec_401()
    {
        var anonymous = _api.CreateApiClient();

        var response = await anonymous.GetAsync(OrdersApi.Path);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>Vraie commande : une chaîne de 45 cm prise sur un rouleau de 250 cm et deux fois la même breloque (stock 3).</summary>
    private async Task<(HttpClient Customer, OrderDto Order, long ChainId, long CharmId)> PlaceOrderAsync()
    {
        var admin = await _api.CreateClientAsAsync(UserRole.Admin);
        var chainId = await admin.CreateQuoteChainAsync(rollLengthCm: RollLengthCm);
        var charm = await admin.CreateQuoteCharmAsync(stock: 3);
        var customer = await _api.CreateCustomerClientAsync();

        var response = await customer.PostAsJsonAsync(OrdersApi.Path, OrdersApi.OrderBody(chainId, charm.Id, charm.Id));
        response.EnsureSuccessStatusCode();
        return (customer, (await response.Content.ReadFromJsonAsync<OrderDto>())!, chainId, charm.Id);
    }

    private Task<string> StatusOfAsync(long orderId) => _api.QueryAsync(async services =>
        (await services.GetRequiredService<AppDbContext>().Orders.AsNoTracking().SingleAsync(o => o.Id == orderId))
            .Status.ToString());

    private Task<List<int>> RemainingRollLengthsAsync(long chainId) => _api.QueryAsync(services =>
        services.GetRequiredService<AppDbContext>().Chains
            .Where(c => c.Id == chainId)
            .SelectMany(c => c.Rolls)
            .Select(r => r.RemainingLengthCm)
            .ToListAsync());

    private Task<(List<string> AdminEmails, List<string> Mails)> AdminMailsAsync(string subjectStart) =>
        _api.QueryAsync(async services =>
        {
            var db = services.GetRequiredService<AppDbContext>();
            var adminEmails = await db.Users.Where(u => u.Role == UserRole.Admin).Select(u => u.Email).ToListAsync();
            var mails = await db.EmailOutboxMessages
                .Where(m => m.Subject.StartsWith(subjectStart))
                .Select(m => m.ToAddress)
                .ToListAsync();
            return (adminEmails, mails);
        });
}

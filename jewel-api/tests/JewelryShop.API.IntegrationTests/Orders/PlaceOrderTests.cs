using System.Net;
using System.Net.Http.Json;
using JewelryShop.API.IntegrationTests.Chains;
using JewelryShop.API.IntegrationTests.Infrastructure;
using JewelryShop.API.IntegrationTests.Users;
using JewelryShop.Common.Enums;
using JewelryShop.Features.Orders.DTOs;
using JewelryShop.Features.Orders.Entities;
using JewelryShop.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace JewelryShop.API.IntegrationTests.Orders;

[Collection(ApiCollection.Name)]
public class PlaceOrderTests
{
    private readonly JewelryShopApiFactory _api;

    public PlaceOrderTests(JewelryShopApiFactory api) => _api = api;

    [Fact]
    public async Task Cree_une_commande_Pending_numerotee_avec_les_prix_copies_et_le_stock_decremente()
    {
        var admin = await _api.CreateClientAsAsync(UserRole.Admin);
        var chainId = await admin.CreateQuoteChainAsync(rollLengthCm: 250);
        var charm = await admin.CreateQuoteCharmAsync(stock: 3);
        var customer = await _api.CreateCustomerClientAsync();

        var response = await customer.PostAsJsonAsync(OrdersApi.Path, OrdersApi.OrderBody(chainId, charm.Id, charm.Id));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var order = await response.Content.ReadFromJsonAsync<OrderDto>();
        Assert.NotNull(order);
        Assert.Equal($"/api/orders/{order.Id}", response.Headers.Location?.OriginalString);
        Assert.Equal("Pending", order.Status);
        Assert.Matches(@"^CMD-\d{4}-\d{4,}$", order.Number);
        Assert.Equal(16.50m, order.Total);
        Assert.Equal(
            [
                new OrderLineDto("Chain", chainId, order.Lines[0].Name, OrdersApi.OfferedLengthCm, null, 10m, 2m, 6.50m),
                new OrderLineDto("Charm", charm.Id, charm.Name, null, 1, null, null, 5m),
                new OrderLineDto("Charm", charm.Id, charm.Name, null, 2, null, null, 5m),
            ],
            order.Lines);
        Assert.Equal(new OrderRecipientDto("Marie", "Test", "0612345678"), order.Recipient);
        Assert.Equal(new OrderAddressDto("12 rue des Lilas", null, "75011", "Paris"), order.Address);
        var creation = Assert.Single(order.StatusHistory);
        Assert.Equal((null, "Pending"), (creation.FromStatus, creation.ToStatus));
        Assert.Equal(1, await customer.GetCharmStockAsync(charm.Id));
        Assert.Equal([250 - OrdersApi.OfferedLengthCm], await RemainingRollLengthsAsync(chainId));
    }

    [Fact]
    public async Task Met_en_file_la_confirmation_au_client_et_un_avis_a_chaque_admin()
    {
        var admin = await _api.CreateClientAsAsync(UserRole.Admin);
        await _api.CreateUserAsync(UserRole.Admin);
        var chainId = await admin.CreateQuoteChainAsync();
        var charm = await admin.CreateQuoteCharmAsync();
        var customer = await _api.CreateCustomerClientAsync();

        // L'envoi est désactivé dans les tests : la commande passe sans serveur de mail.
        var response = await customer.PostAsJsonAsync(OrdersApi.Path, OrdersApi.OrderBody(chainId, charm.Id));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var order = await response.Content.ReadFromJsonAsync<OrderDto>();
        Assert.NotNull(order);
        var (customerEmail, adminEmails, mails) = await _api.QueryAsync(async services =>
        {
            var db = services.GetRequiredService<AppDbContext>();
            var userId = await db.Orders.Where(o => o.Id == order.Id).Select(o => o.UserId).SingleAsync();
            var customerEmail = await db.Users.Where(u => u.Id == userId).Select(u => u.Email).SingleAsync();
            var adminEmails = await db.Users.Where(u => u.Role == UserRole.Admin).Select(u => u.Email).ToListAsync();
            var mails = await db.EmailOutboxMessages
                .Where(m => m.Subject.Contains(order.Number))
                .Select(m => new { m.ToAddress, m.Subject, m.TextBody })
                .ToListAsync();
            return (customerEmail, adminEmails, mails);
        });
        var confirmation = Assert.Single(mails, m => m.ToAddress == customerEmail);
        Assert.Equal($"Votre commande {order.Number} est bien enregistrée", confirmation.Subject);
        Assert.Contains($"/orders/{order.Id}", confirmation.TextBody);
        var notices = mails.Where(m => m.Subject.StartsWith("Nouvelle commande")).ToList();
        Assert.True(adminEmails.Count >= 2);
        Assert.Equal(adminEmails.Order(), notices.Select(m => m.ToAddress).Order());
        Assert.All(notices, m => Assert.Contains($"/admin/orders/{order.Id}", m.TextBody));
    }

    [Fact]
    public async Task Prend_45_cm_dans_le_rouleau_de_50_plutot_que_dans_celui_de_200()
    {
        var admin = await _api.CreateClientAsAsync(UserRole.Admin);
        var chainId = await admin.CreateQuoteChainAsync(rollLengthCm: 200);
        await AddRollAsync(chainId, 50);
        var charm = await admin.CreateQuoteCharmAsync();
        var customer = await _api.CreateCustomerClientAsync();

        var response = await customer.PostAsJsonAsync(OrdersApi.Path, OrdersApi.OrderBody(chainId, charm.Id));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal([5, 200], await RemainingRollLengthsAsync(chainId));
    }

    [Fact]
    public async Task Accepte_une_seule_de_deux_commandes_simultanees_sur_la_derniere_breloque()
    {
        var admin = await _api.CreateClientAsAsync(UserRole.Admin);
        var chainId = await admin.CreateQuoteChainAsync();
        var charm = await admin.CreateQuoteCharmAsync(stock: 1);
        var first = await _api.CreateCustomerClientAsync();
        var second = await _api.CreateCustomerClientAsync();
        var body = OrdersApi.OrderBody(chainId, charm.Id);

        var responses = await Task.WhenAll(
            first.PostAsJsonAsync(OrdersApi.Path, body),
            second.PostAsJsonAsync(OrdersApi.Path, body));

        Assert.Equal(
            [HttpStatusCode.Created, HttpStatusCode.Conflict],
            responses.Select(r => r.StatusCode).Order());
        var refused = responses.Single(r => r.StatusCode == HttpStatusCode.Conflict);
        var error = await refused.Content.ReadFromJsonAsync<ApiError>();
        Assert.Equal($"Le stock de « {charm.Name} » ne permet plus cette commande.", error?.Title);
        Assert.Equal(0, await first.GetCharmStockAsync(charm.Id));
        Assert.Equal(1, await CountOrdersWithCharmAsync(charm.Id));
    }

    [Fact]
    public async Task N_enregistre_rien_quand_la_deuxieme_breloque_est_epuisee_pendant_la_commande()
    {
        var admin = await _api.CreateClientAsAsync(UserRole.Admin);
        var chainId = await admin.CreateQuoteChainAsync(rollLengthCm: 250);
        var clover = await admin.CreateQuoteCharmAsync(stock: 5);
        var heart = await admin.CreateQuoteCharmAsync(stock: 1);
        var customer = await _api.CreateCustomerClientAsync();

        // Une autre transaction verrouille le dernier « cœur » : la commande prélève la chaîne et le trèfle,
        // attend ce verrou, puis trouve le stock à 0 une fois l'autre transaction validée.
        await using var connection = await OpenConnectionAsync();
        await using var concurrentSale = await connection.BeginTransactionAsync();
        await using (var sell = new NpgsqlCommand("UPDATE charms SET stock = 0 WHERE id = @id", connection, concurrentSale))
        {
            sell.Parameters.AddWithValue("id", heart.Id);
            await sell.ExecuteNonQueryAsync();
        }

        var pending = customer.PostAsJsonAsync(OrdersApi.Path, OrdersApi.OrderBody(chainId, clover.Id, heart.Id));
        await WaitForAnUpdateBlockedOnALockAsync();
        await concurrentSale.CommitAsync();
        var response = await pending;

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ApiError>();
        Assert.Equal($"Le stock de « {heart.Name} » ne permet plus cette commande.", error?.Title);
        Assert.Equal(5, await customer.GetCharmStockAsync(clover.Id));
        Assert.Equal([250], await RemainingRollLengthsAsync(chainId));
        Assert.Equal(0, await CountOrdersWithCharmAsync(clover.Id));
    }

    [Fact]
    public async Task Refuse_avec_400_un_client_sans_adresse_qui_n_en_saisit_pas()
    {
        var admin = await _api.CreateClientAsAsync(UserRole.Admin);
        var chainId = await admin.CreateQuoteChainAsync();
        var charm = await admin.CreateQuoteCharmAsync(stock: 2);
        var user = await _api.CreateClientAsAsync(UserRole.User);
        var body = new
        {
            chainId,
            lengthCm = OrdersApi.OfferedLengthCm,
            charmIds = new[] { charm.Id },
            recipient = new { name = "Alice", surname = "Martin", phone = "0612345678" }
        };

        var response = await user.PostAsJsonAsync(OrdersApi.Path, body);

        await AssertValidationErrorAsync(response, "Address", "Veuillez renseigner votre adresse de livraison.");
        Assert.Equal(2, await user.GetCharmStockAsync(charm.Id));
    }

    [Fact]
    public async Task Refuse_avec_400_une_commande_sans_aucun_telephone()
    {
        var admin = await _api.CreateClientAsAsync(UserRole.Admin);
        var chainId = await admin.CreateQuoteChainAsync();
        var charm = await admin.CreateQuoteCharmAsync();
        var user = await _api.CreateClientAsAsync(UserRole.User);
        var body = new
        {
            chainId,
            lengthCm = OrdersApi.OfferedLengthCm,
            charmIds = new[] { charm.Id },
            address = new { line1 = "3 place du Marché", postalCode = "69002", city = "Lyon" }
        };

        var response = await user.PostAsJsonAsync(OrdersApi.Path, body);

        await AssertValidationErrorAsync(response, "Recipient.Phone", "Le téléphone est requis.");
    }

    [Fact]
    public async Task Refuse_avec_400_un_commentaire_de_1001_caracteres()
    {
        var admin = await _api.CreateClientAsAsync(UserRole.Admin);
        var chainId = await admin.CreateQuoteChainAsync();
        var charm = await admin.CreateQuoteCharmAsync();
        var customer = await _api.CreateCustomerClientAsync();
        var body = new
        {
            chainId,
            lengthCm = OrdersApi.OfferedLengthCm,
            charmIds = new[] { charm.Id },
            comment = new string('a', 1001)
        };

        var response = await customer.PostAsJsonAsync(OrdersApi.Path, body);

        await AssertValidationErrorAsync(response, "Comment", "1000 caractères maximum.");
    }

    [Fact]
    public async Task Remplace_l_adresse_du_profil_quand_saveAddressToProfile_est_vrai()
    {
        var admin = await _api.CreateClientAsAsync(UserRole.Admin);
        var chainId = await admin.CreateQuoteChainAsync();
        var charm = await admin.CreateQuoteCharmAsync();
        var customer = await _api.CreateCustomerClientAsync();
        var body = new
        {
            chainId,
            lengthCm = OrdersApi.OfferedLengthCm,
            charmIds = new[] { charm.Id },
            address = new { line1 = "3 place du Marché", line2 = "2e étage", postalCode = "69002", city = "Lyon" },
            saveAddressToProfile = true
        };

        var response = await customer.PostAsJsonAsync(OrdersApi.Path, body);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var profile = await customer.GetMyProfileAsync();
        Assert.Equal("3 place du Marché", profile.Address?.Line1);
        Assert.Equal("2e étage", profile.Address?.Line2);
        Assert.Equal("69002", profile.Address?.PostalCode);
        Assert.Equal("Lyon", profile.Address?.City);
        Assert.Equal("0612345678", profile.Phone);
    }

    [Fact]
    public async Task Garde_l_adresse_du_profil_quand_l_adresse_saisie_ne_sert_qu_a_cette_commande()
    {
        var admin = await _api.CreateClientAsAsync(UserRole.Admin);
        var chainId = await admin.CreateQuoteChainAsync();
        var charm = await admin.CreateQuoteCharmAsync();
        var customer = await _api.CreateCustomerClientAsync();
        var body = new
        {
            chainId,
            lengthCm = OrdersApi.OfferedLengthCm,
            charmIds = new[] { charm.Id },
            address = new { line1 = "3 place du Marché", postalCode = "69002", city = "Lyon" }
        };

        var response = await customer.PostAsJsonAsync(OrdersApi.Path, body);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var order = await response.Content.ReadFromJsonAsync<OrderDto>();
        Assert.Equal("Lyon", order?.Address.City);
        Assert.Equal("Paris", (await customer.GetMyProfileAsync()).Address?.City);
    }

    [Fact]
    public async Task Garde_l_ancien_prix_d_une_breloque_dont_le_prix_change_apres_la_commande()
    {
        var admin = await _api.CreateClientAsAsync(UserRole.Admin);
        var chainId = await admin.CreateQuoteChainAsync();
        var charm = await admin.CreateQuoteCharmAsync();
        var customer = await _api.CreateCustomerClientAsync();
        var response = await customer.PostAsJsonAsync(OrdersApi.Path, OrdersApi.OrderBody(chainId, charm.Id));
        var order = (await response.Content.ReadFromJsonAsync<OrderDto>())!;

        await _api.SeedAsync(services => services.GetRequiredService<AppDbContext>().Database
            .ExecuteSqlAsync($"UPDATE charms SET price = 12, name = 'Renommée' WHERE id = {charm.Id}"));

        var saved = await _api.QueryAsync(services => services.GetRequiredService<AppDbContext>().Orders
            .Include(o => o.Lines)
            .AsNoTracking()
            .SingleAsync(o => o.Id == order.Id));
        var charmLine = saved.Lines.Single(l => l.Kind == OrderLineKind.Charm);
        Assert.Equal((5m, charm.Name), (charmLine.UnitPrice, charmLine.Name));
        Assert.Equal(11.50m, saved.Total);
    }

    [Fact]
    public async Task Refuse_un_appel_anonyme_avec_401()
    {
        var response = await _api.CreateApiClient().PostAsJsonAsync(OrdersApi.Path, OrdersApi.OrderBody(1, 1));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private Task AddRollAsync(long chainId, int lengthCm) => _api.SeedAsync(async services =>
    {
        var context = services.GetRequiredService<AppDbContext>();
        var chain = await context.Chains.SingleAsync(c => c.Id == chainId);
        chain.AddRoll(lengthCm, DateTime.UtcNow);
        await context.SaveChangesAsync();
    });

    private Task<List<int>> RemainingRollLengthsAsync(long chainId) => _api.QueryAsync(services =>
        services.GetRequiredService<AppDbContext>().Chains
            .Where(c => c.Id == chainId)
            .SelectMany(c => c.Rolls)
            .Select(r => r.RemainingLengthCm)
            .OrderBy(length => length)
            .ToListAsync());

    private Task<int> CountOrdersWithCharmAsync(long charmId) => _api.QueryAsync(services =>
        services.GetRequiredService<AppDbContext>().OrderLines.CountAsync(l => l.CharmId == charmId));

    private async Task<NpgsqlConnection> OpenConnectionAsync()
    {
        var connectionString = await _api.QueryAsync(services =>
            Task.FromResult(services.GetRequiredService<AppDbContext>().Database.GetConnectionString()!));
        var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        return connection;
    }

    // Attend que la commande soit bloquée sur la ligne verrouillée par le test, sans dépendre d'un délai fixe.
    private async Task WaitForAnUpdateBlockedOnALockAsync()
    {
        await using var connection = await OpenConnectionAsync();
        await using var blocked = new NpgsqlCommand(
            "SELECT count(*) FROM pg_stat_activity WHERE wait_event_type = 'Lock' AND query LIKE 'UPDATE charms%'",
            connection);

        var deadline = DateTime.UtcNow.AddSeconds(30);
        while ((long)(await blocked.ExecuteScalarAsync())! == 0)
        {
            if (DateTime.UtcNow > deadline)
                throw new TimeoutException("La commande n'a jamais attendu le verrou de la breloque.");
            await Task.Delay(50);
        }
    }

    private static async Task AssertValidationErrorAsync(HttpResponseMessage response, string field, string message)
    {
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ApiError>();
        Assert.NotNull(error?.Errors);
        Assert.Contains(message, Assert.Contains(field, error.Errors));
    }
}

using System.Net;
using JewelryShop.API.Tests.Fakes;
using JewelryShop.Common;
using JewelryShop.Common.Enums;
using JewelryShop.Features.Emails.Entities;
using JewelryShop.Features.Orders.Common;
using JewelryShop.Features.Orders.Entities;
using JewelryShop.Features.Users.Entities;
using JewelryShop.Infrastructure.Email;

namespace JewelryShop.API.Tests.Orders;

public class OrderCustomerActionEmailsTests
{
    private const string FirstAdminEmail = "admin1@mycharmly.eu";
    private const string SecondAdminEmail = "admin2@mycharmly.eu";

    private static readonly DateTime _now = new(2026, 11, 17, 9, 30, 0, DateTimeKind.Utc);

    private static readonly User _customer =
        new() { Id = 4, Name = "Marie", Surname = "Durand", Email = "marie@example.com" };

    private static readonly User[] _admins =
    [
        new() { Id = 1, Name = "Alice", Surname = "Admin", Email = FirstAdminEmail, Role = UserRole.Admin },
        new() { Id = 2, Name = "Bruno", Surname = "Admin", Email = SecondAdminEmail, Role = UserRole.Admin }
    ];

    private readonly FakeEmailOutboxRepository _outbox = new();

    [Theory]
    [InlineData(EmailTemplates.OrderCancelledByCustomerAdmin)]
    [InlineData(EmailTemplates.OrderReceivedAdmin)]
    public void Met_en_file_un_avis_par_admin_et_aucun_au_client(string template)
    {
        Enqueue(template);

        Assert.Equal([FirstAdminEmail, SecondAdminEmail], _outbox.All.Select(m => m.ToAddress));
        Assert.All(_outbox.All, m => Assert.Equal(EmailOutboxStatus.Pending, m.Status));
    }

    [Fact]
    public void Ne_met_rien_en_file_sans_admin()
    {
        Enqueue(EmailTemplates.OrderReceivedAdmin, admins: []);

        Assert.Empty(_outbox.All);
    }

    [Fact]
    public void Donne_a_l_admin_l_annulation_les_articles_et_le_lien_vers_la_page_admin()
    {
        Enqueue(EmailTemplates.OrderCancelledByCustomerAdmin);

        var mail = FirstAdminMail;
        Assert.Equal("Commande CMD-2026-0042 annulée par Marie Durand", mail.Subject);
        Assert.Contains("Marie Durand (marie@example.com) a annulé la commande CMD-2026-0042 du 17/11/2026.", mail.TextBody);
        Assert.Contains(
            """
            Articles :
            - Chaîne Forçat, 45 cm : 6,50 €
            - Breloque 1 : Trèfle : 5,00 €
            Total : 11,50 €
            """.ReplaceLineEndings(),
            mail.TextBody.ReplaceLineEndings());
        Assert.Contains("Le stock de la chaîne et des breloques a été recrédité.", mail.TextBody);
        Assert.Contains("http://localhost:5173/admin/orders/42", mail.TextBody);
        Assert.Contains("<a href=\"http://localhost:5173/admin/orders/42\">", mail.HtmlBody);
        Assert.Contains("<li>Chaîne Forçat, 45 cm : 6,50 €</li>", WebUtility.HtmlDecode(mail.HtmlBody));
    }

    [Fact]
    public void Donne_a_l_admin_la_reception_et_le_lien_vers_la_page_admin()
    {
        Enqueue(EmailTemplates.OrderReceivedAdmin);

        var mail = FirstAdminMail;
        Assert.Equal("Commande CMD-2026-0042 reçue par Marie Durand", mail.Subject);
        Assert.Contains(
            "Marie Durand (marie@example.com) a confirmé avoir reçu la commande CMD-2026-0042 du 17/11/2026.",
            mail.TextBody);
        Assert.Contains("Total : 11,50 €", mail.TextBody);
        Assert.Contains("<a href=\"http://localhost:5173/admin/orders/42\">", mail.HtmlBody);
    }

    [Fact]
    public void Echappe_le_nom_du_client_dans_le_html()
    {
        var customer = new User { Id = 4, Name = "<b>Marie</b>", Surname = "Durand", Email = "marie@example.com" };

        Enqueue(EmailTemplates.OrderCancelledByCustomerAdmin, customer: customer);

        Assert.Contains("&lt;b&gt;Marie&lt;/b&gt;", FirstAdminMail.HtmlBody);
    }

    private EmailOutboxMessage FirstAdminMail => _outbox.All.Single(m => m.ToAddress == FirstAdminEmail);

    private void Enqueue(string template, User[]? admins = null, User? customer = null)
    {
        var emailQueue = new EmailQueue(new ScribanEmailTemplateRenderer(), _outbox, new FakeTimeProvider(_now));
        OrderCustomerActionEmails.Enqueue(
            emailQueue, new AppOptions(), template, NewOrder(), customer ?? _customer, admins ?? _admins);
    }

    private static Order NewOrder() => new()
    {
        Id = 42,
        Number = "CMD-2026-0042",
        UserId = _customer.Id,
        RecipientName = "Alice",
        RecipientSurname = "Martin",
        RecipientPhone = "0612345678",
        AddressLine1 = "12 rue des Lilas",
        PostalCode = "75011",
        City = "Paris",
        Total = 11.5m,
        CreatedAt = _now,
        UpdatedAt = _now,
        Lines =
        [
            new OrderLine { Kind = OrderLineKind.Charm, CharmId = 10, Name = "Trèfle", UnitPrice = 5m, Position = 1 },
            new OrderLine { Kind = OrderLineKind.Chain, ChainId = 3, Name = "Forçat", LengthCm = 45, UnitPrice = 6.5m }
        ]
    };
}

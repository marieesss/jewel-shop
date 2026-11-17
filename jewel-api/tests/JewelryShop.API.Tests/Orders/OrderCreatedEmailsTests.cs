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

public class OrderCreatedEmailsTests
{
    private const string CustomerEmail = "marie@example.com";
    private const string FirstAdminEmail = "admin1@mycharmly.eu";
    private const string SecondAdminEmail = "admin2@mycharmly.eu";

    private static readonly DateTime _now = new(2026, 11, 17, 9, 30, 0, DateTimeKind.Utc);

    private static readonly User _customer = new() { Id = 4, Name = "Marie", Surname = "Durand", Email = CustomerEmail };

    private static readonly User[] _admins =
    [
        new() { Id = 1, Name = "Alice", Surname = "Admin", Email = FirstAdminEmail, Role = UserRole.Admin },
        new() { Id = 2, Name = "Bruno", Surname = "Admin", Email = SecondAdminEmail, Role = UserRole.Admin }
    ];

    private readonly FakeEmailOutboxRepository _outbox = new();

    [Fact]
    public void Met_en_file_la_confirmation_au_client_et_un_avis_par_admin()
    {
        Enqueue(NewOrder());

        Assert.Equal(
            [CustomerEmail, FirstAdminEmail, SecondAdminEmail],
            _outbox.All.Select(m => m.ToAddress));
        Assert.All(_outbox.All, m => Assert.Equal(EmailOutboxStatus.Pending, m.Status));
    }

    [Fact]
    public void Met_en_file_seulement_la_confirmation_sans_admin()
    {
        Enqueue(NewOrder(), admins: []);

        Assert.Equal(CustomerEmail, Assert.Single(_outbox.All).ToAddress);
    }

    [Fact]
    public void Donne_au_client_un_sujet_avec_le_numero_de_commande()
    {
        Enqueue(NewOrder());

        Assert.Equal("Votre commande CMD-2026-0042 est bien enregistrée", CustomerMail.Subject);
    }

    [Fact]
    public void Liste_au_client_les_articles_dans_l_ordre_du_collier_avec_leur_prix_et_le_total()
    {
        Enqueue(NewOrder());

        Assert.Contains(
            """
            Articles :
            - Chaîne Forçat, 45 cm : 6,50 €
            - Breloque 1 : Cœur : 4,50 €
            - Breloque 2 : Trèfle : 5,00 €
            Total : 16,00 €
            """.ReplaceLineEndings(),
            CustomerMail.TextBody.ReplaceLineEndings());
    }

    [Fact]
    public void Donne_au_client_le_numero_la_date_le_destinataire_et_l_adresse()
    {
        Enqueue(NewOrder());

        var text = CustomerMail.TextBody.ReplaceLineEndings();
        Assert.StartsWith("Bonjour Marie,", text);
        Assert.Contains("Nous avons bien reçu votre commande CMD-2026-0042 du 17/11/2026.", text);
        Assert.Contains(
            """
            Livraison à :
            Alice Martin
            12 rue des Lilas
            Bâtiment B
            75011 Paris
            Téléphone : 0612345678
            """.ReplaceLineEndings(),
            text);
    }

    [Fact]
    public void Omet_le_complement_d_adresse_absent()
    {
        var order = NewOrder();
        order.AddressLine2 = null;

        Enqueue(order);

        Assert.Contains(
            "12 rue des Lilas" + Environment.NewLine + "75011 Paris",
            CustomerMail.TextBody.ReplaceLineEndings());
    }

    [Fact]
    public void Donne_au_client_son_commentaire_et_le_lien_vers_sa_commande()
    {
        Enqueue(NewOrder());

        Assert.Contains("Votre commentaire : Gravure au dos", CustomerMail.TextBody);
        Assert.Contains("http://localhost:5173/orders/42", CustomerMail.TextBody);
        Assert.Contains("<a href=\"http://localhost:5173/orders/42\">", CustomerMail.HtmlBody);
    }

    [Fact]
    public void N_affiche_pas_de_commentaire_quand_le_client_n_en_a_pas_laisse()
    {
        var order = NewOrder();
        order.Comment = null;

        Enqueue(order);

        Assert.DoesNotContain("commentaire", CustomerMail.TextBody);
        Assert.DoesNotContain("commentaire", CustomerMail.HtmlBody);
    }

    [Fact]
    public void Liste_au_client_les_articles_dans_l_ordre_dans_le_html()
    {
        Enqueue(NewOrder());

        // html.escape encode aussi les lettres accentuées (« &#238; ») : on compare le texte décodé.
        var html = WebUtility.HtmlDecode(CustomerMail.HtmlBody);
        Assert.Contains("<li>Chaîne Forçat, 45 cm : 6,50 €</li>", html);
        Assert.True(html.IndexOf("Breloque 1 : Cœur", StringComparison.Ordinal)
            < html.IndexOf("Breloque 2 : Trèfle", StringComparison.Ordinal));
        Assert.Contains("Total : 16,00 €", html);
        Assert.Contains("75011 Paris", html);
    }

    [Fact]
    public void Echappe_le_commentaire_dans_le_html()
    {
        var order = NewOrder();
        order.Comment = "<b>Merci</b>";

        Enqueue(order);

        Assert.Contains("&lt;b&gt;Merci&lt;/b&gt;", CustomerMail.HtmlBody);
    }

    [Fact]
    public void Donne_a_l_admin_le_client_les_articles_et_le_lien_vers_la_page_admin()
    {
        Enqueue(NewOrder());

        var mail = _outbox.All.Single(m => m.ToAddress == FirstAdminEmail);
        Assert.Equal("Nouvelle commande CMD-2026-0042 de Marie Durand", mail.Subject);
        Assert.Contains("Marie Durand (marie@example.com) a passé la commande CMD-2026-0042 le 17/11/2026.", mail.TextBody);
        Assert.Contains("- Breloque 2 : Trèfle : 5,00 €", mail.TextBody);
        Assert.Contains("Total : 16,00 €", mail.TextBody);
        Assert.Contains("75011 Paris", mail.TextBody);
        Assert.Contains("Commentaire du client : Gravure au dos", mail.TextBody);
        Assert.Contains("http://localhost:5173/admin/orders/42", mail.TextBody);
        Assert.Contains("<a href=\"http://localhost:5173/admin/orders/42\">", mail.HtmlBody);
    }

    [Theory]
    [InlineData(0, "0,00 €")]
    [InlineData(4.5, "4,50 €")]
    [InlineData(1234.56, "1234,56 €")]
    public void Formate_les_prix_en_euros_avec_une_virgule(decimal price, string expected)
    {
        Assert.Equal(expected, OrderEmailSummary.FormatPrice(price));
    }

    private EmailOutboxMessage CustomerMail => _outbox.All.Single(m => m.ToAddress == CustomerEmail);

    private void Enqueue(Order order, User[]? admins = null)
    {
        var emailQueue = new EmailQueue(new ScribanEmailTemplateRenderer(), _outbox, new FakeTimeProvider(_now));
        OrderCreatedEmails.Enqueue(emailQueue, new AppOptions(), order, _customer, admins ?? _admins);
    }

    // Lignes volontairement dans le désordre : le mail doit suivre la position des breloques.
    private static Order NewOrder() => new()
    {
        Id = 42,
        Number = "CMD-2026-0042",
        UserId = _customer.Id,
        Comment = "Gravure au dos",
        RecipientName = "Alice",
        RecipientSurname = "Martin",
        RecipientPhone = "0612345678",
        AddressLine1 = "12 rue des Lilas",
        AddressLine2 = "Bâtiment B",
        PostalCode = "75011",
        City = "Paris",
        Total = 16m,
        CreatedAt = _now,
        UpdatedAt = _now,
        Lines =
        [
            new OrderLine { Kind = OrderLineKind.Charm, CharmId = 10, Name = "Trèfle", UnitPrice = 5m, Position = 2 },
            new OrderLine { Kind = OrderLineKind.Chain, ChainId = 3, Name = "Forçat", LengthCm = 45, UnitPrice = 6.5m },
            new OrderLine { Kind = OrderLineKind.Charm, CharmId = 11, Name = "Cœur", UnitPrice = 4.5m, Position = 1 }
        ]
    };
}

using JewelryShop.API.Tests.Fakes;
using JewelryShop.Common.Enums;
using JewelryShop.Common.Exceptions;
using JewelryShop.Features.ChainLengths.Entities;
using JewelryShop.Features.Chains.Entities;
using JewelryShop.Features.Charms.Entities;
using JewelryShop.Features.Orders.Commands;
using JewelryShop.Features.Orders.Common;
using JewelryShop.Features.Orders.DTOs;
using JewelryShop.Features.Orders.Entities;
using JewelryShop.Features.Users.Entities;

namespace JewelryShop.API.Tests.Orders;

public class PlaceOrderCommandHandlerTests
{
    private const long CustomerId = 4;
    private const long NewcomerId = 5;
    private const long SmallRollId = 100;
    private const long LargeRollId = 101;

    private static readonly DateTime _now = new(2026, 11, 17, 9, 30, 0, DateTimeKind.Utc);

    private readonly FakeOrderRepository _orders = new();
    private readonly FakeUserRepository _users = new();
    private readonly FakeChainRepository _chains = new();
    private readonly FakeChainLengthRepository _lengths = new();
    private readonly FakeCharmRepository _charms = new();
    private readonly PlaceOrderCommandHandler _handler;

    public PlaceOrderCommandHandlerTests()
    {
        _handler = new PlaceOrderCommandHandler(
            _orders, _users, new NecklaceComposer(_chains, _lengths, _charms), new FakeTimeProvider(_now));

        var chain = new Chain { Id = 3, Name = "Forçat", Color = ProductColor.Gold, PricePerMeter = 10m, ClaspPrice = 2m };
        chain.AddRoll(50, _now);
        chain.AddRoll(200, _now);
        _chains.Seed(chain);
        _orders.Rolls[SmallRollId] = (3, 50);
        _orders.Rolls[LargeRollId] = (3, 200);

        _lengths.Seed(new ChainLength { Id = 1, LengthCm = 45, Label = "Princesse" });

        _charms.Seed(
            new Charm { Id = 10, Name = "Trèfle", Color = ProductColor.Silver, Price = 5m, Stock = 2 },
            new Charm { Id = 11, Name = "Cœur", Color = ProductColor.Silver, Price = 4.5m, Stock = 5 });
        _orders.CharmStocks[10] = 2;
        _orders.CharmStocks[11] = 5;

        _users.Seed(
            new User
            {
                Id = CustomerId,
                Name = "Marie",
                Surname = "Durand",
                Email = "marie@example.com",
                Phone = "0612345678",
                AddressLine1 = "12 rue des Lilas",
                PostalCode = "75011",
                City = "Paris"
            },
            new User { Id = NewcomerId, Name = "Léa", Surname = "Petit", Email = "lea@example.com" });
    }

    private static PlaceOrderCommand Command(long userId = CustomerId, params long[] charmIds) =>
        new(userId, 3, 45, charmIds.Length == 0 ? [11, 10, 10] : charmIds, null, null, null, false);

    private static readonly OrderAddressInput _otherAddress = new("  3 place du Marché ", " ", "69002", " Lyon ");

    [Fact]
    public async Task Cree_une_commande_Pending_numerotee_avec_le_total_du_collier()
    {
        var order = await _handler.Handle(Command(), CancellationToken.None);

        Assert.Equal("Pending", order.Status);
        Assert.Equal("CMD-2026-0001", order.Number);
        Assert.Equal(6.50m + 4.5m + 5m + 5m, order.Total);
        Assert.Equal(_now, order.CreatedAt);
        var saved = Assert.Single(_orders.All);
        Assert.Equal(OrderStatus.Pending, saved.Status);
        Assert.Equal(CustomerId, saved.UserId);
        Assert.Equal(1, _orders.SaveChangesCount);
        Assert.Equal(1, _orders.TransactionCount);
    }

    [Fact]
    public async Task Copie_le_nom_et_les_prix_de_chaque_ligne_avec_le_rouleau_et_les_positions()
    {
        var order = await _handler.Handle(Command(), CancellationToken.None);

        Assert.Equal(
            [
                new OrderLineDto("Chain", 3, "Forçat", 45, null, 10m, 2m, 6.50m),
                new OrderLineDto("Charm", 11, "Cœur", null, 1, null, null, 4.5m),
                new OrderLineDto("Charm", 10, "Trèfle", null, 2, null, null, 5m),
                new OrderLineDto("Charm", 10, "Trèfle", null, 3, null, null, 5m),
            ],
            order.Lines);
        var chainLine = _orders.All.Single().Lines.Single(l => l.Kind == OrderLineKind.Chain);
        Assert.Equal(SmallRollId, chainLine.ChainRollId);
    }

    [Fact]
    public async Task Prend_les_45_cm_dans_le_plus_petit_rouleau_qui_les_contient()
    {
        await _handler.Handle(Command(), CancellationToken.None);

        Assert.Equal(5, _orders.Rolls[SmallRollId].RemainingCm);
        Assert.Equal(200, _orders.Rolls[LargeRollId].RemainingCm);
    }

    [Fact]
    public async Task Retire_une_unite_de_stock_par_exemplaire_de_breloque()
    {
        await _handler.Handle(Command(), CancellationToken.None);

        Assert.Equal(0, _orders.CharmStocks[10]);
        Assert.Equal(4, _orders.CharmStocks[11]);
    }

    [Fact]
    public async Task Ecrit_la_creation_dans_l_historique_sans_notifier_le_client()
    {
        var order = await _handler.Handle(Command(), CancellationToken.None);

        Assert.Equal([new OrderStatusChangeDto(null, "Pending", _now)], order.StatusHistory);
        var history = Assert.Single(_orders.All.Single().StatusHistory);
        Assert.Equal(CustomerId, history.ChangedBy);
        Assert.False(history.CustomerNotified);
    }

    [Fact]
    public async Task Refuse_avec_409_et_n_enregistre_rien_quand_la_deuxieme_breloque_n_a_plus_de_stock()
    {
        // Une commande simultanée a pris le dernier « Cœur » après la vérification du composeur.
        _orders.CharmStocks[11] = 0;

        var error = await Assert.ThrowsAsync<ConflictException>(
            () => _handler.Handle(Command(CustomerId, 10, 11), CancellationToken.None));

        Assert.Equal("Le stock de « Cœur » ne permet plus cette commande.", error.Message);
        Assert.Equal(2, _orders.CharmStocks[10]);
        Assert.Equal(50, _orders.Rolls[SmallRollId].RemainingCm);
        Assert.Empty(_orders.All);
        Assert.Equal(0, _orders.SaveChangesCount);
    }

    [Fact]
    public async Task Refuse_avec_409_quand_plus_aucun_rouleau_ne_contient_la_longueur()
    {
        _orders.Rolls[SmallRollId] = (3, 40);
        _orders.Rolls[LargeRollId] = (3, 44);

        var error = await Assert.ThrowsAsync<ConflictException>(
            () => _handler.Handle(Command(), CancellationToken.None));

        Assert.Equal("Le stock de « Forçat » ne permet plus cette commande.", error.Message);
        Assert.Equal(2, _orders.CharmStocks[10]);
        Assert.Empty(_orders.All);
    }

    [Fact]
    public async Task Livre_le_titulaire_du_compte_a_l_adresse_du_profil_par_defaut()
    {
        var order = await _handler.Handle(Command(), CancellationToken.None);

        Assert.Equal(new OrderRecipientDto("Marie", "Durand", "0612345678"), order.Recipient);
        Assert.Equal(new OrderAddressDto("12 rue des Lilas", null, "75011", "Paris"), order.Address);
    }

    [Fact]
    public async Task Livre_un_autre_destinataire_avec_le_telephone_du_compte_s_il_n_en_donne_pas()
    {
        var command = Command() with { Recipient = new OrderRecipientInput(" Alice ", " Martin ", null) };

        var order = await _handler.Handle(command, CancellationToken.None);

        Assert.Equal(new OrderRecipientDto("Alice", "Martin", "0612345678"), order.Recipient);
    }

    [Fact]
    public async Task Garde_le_telephone_du_destinataire_s_il_en_donne_un()
    {
        var command = Command() with { Recipient = new OrderRecipientInput("Alice", "Martin", "+33698765432") };

        var order = await _handler.Handle(command, CancellationToken.None);

        Assert.Equal("+33698765432", order.Recipient.Phone);
    }

    [Fact]
    public async Task Refuse_avec_400_une_commande_sans_aucun_telephone()
    {
        var command = Command(NewcomerId) with { Address = _otherAddress };

        var error = await Assert.ThrowsAsync<ValidationException>(() => _handler.Handle(command, CancellationToken.None));

        Assert.Equal(["Le téléphone est requis."], error.Errors["Recipient.Phone"]);
        Assert.Empty(_orders.All);
    }

    [Fact]
    public async Task Refuse_avec_400_une_commande_sans_adresse_quand_le_profil_n_en_a_pas()
    {
        var command = Command(NewcomerId) with { Recipient = new OrderRecipientInput("Léa", "Petit", "0611111111") };

        var error = await Assert.ThrowsAsync<ValidationException>(() => _handler.Handle(command, CancellationToken.None));

        Assert.Equal(["Veuillez renseigner votre adresse de livraison."], error.Errors["Address"]);
        Assert.Equal(2, _orders.CharmStocks[10]);
    }

    [Fact]
    public async Task Livre_a_l_adresse_saisie_sans_toucher_au_profil()
    {
        var order = await _handler.Handle(Command() with { Address = _otherAddress }, CancellationToken.None);

        Assert.Equal(new OrderAddressDto("3 place du Marché", null, "69002", "Lyon"), order.Address);
        var profile = _users.All.Single(u => u.Id == CustomerId);
        Assert.Equal("12 rue des Lilas", profile.AddressLine1);
    }

    [Fact]
    public async Task Remplace_l_adresse_du_profil_sans_toucher_au_telephone_si_demande()
    {
        var command = Command() with
        {
            Recipient = new OrderRecipientInput("Alice", "Martin", "0698765432"),
            Address = _otherAddress,
            SaveAddressToProfile = true
        };

        await _handler.Handle(command, CancellationToken.None);

        var profile = _users.All.Single(u => u.Id == CustomerId);
        Assert.Equal(
            ("3 place du Marché", (string?)null, "69002", "Lyon", "0612345678"),
            (profile.AddressLine1, profile.AddressLine2, profile.PostalCode, profile.City, profile.Phone));
    }

    [Fact]
    public async Task Ignore_l_enregistrement_sur_le_profil_sans_adresse_saisie()
    {
        var order = await _handler.Handle(Command() with { SaveAddressToProfile = true }, CancellationToken.None);

        Assert.Equal("12 rue des Lilas", order.Address.Line1);
        Assert.Equal("12 rue des Lilas", _users.All.Single(u => u.Id == CustomerId).AddressLine1);
    }

    [Theory]
    [InlineData("  Gravure au dos  ", "Gravure au dos")]
    [InlineData("   ", null)]
    [InlineData(null, null)]
    public async Task Enregistre_le_commentaire_sans_espaces_autour_et_vide_comme_absent(string? comment, string? expected)
    {
        var order = await _handler.Handle(Command() with { Comment = comment }, CancellationToken.None);

        Assert.Equal(expected, order.Comment);
    }

    [Fact]
    public async Task Refuse_avec_404_un_utilisateur_inconnu()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(Command(99), CancellationToken.None));

        Assert.Empty(_orders.All);
    }
}

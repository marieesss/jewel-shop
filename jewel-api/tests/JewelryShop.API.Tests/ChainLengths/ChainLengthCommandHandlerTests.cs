using JewelryShop.API.Tests.Fakes;
using JewelryShop.Common.Exceptions;
using JewelryShop.Features.ChainLengths.Commands;
using JewelryShop.Features.ChainLengths.Common;
using JewelryShop.Features.ChainLengths.Entities;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace JewelryShop.API.Tests.ChainLengths;

public class ChainLengthCommandHandlerTests
{
    private static DbUpdateException UniqueViolation()
        => new("Violation d'unicité.", new PostgresException("duplicate key", "ERROR", "ERROR", PostgresErrorCodes.UniqueViolation));

    private readonly FakeChainLengthRepository _lengths = new();
    private readonly ChainLength _princesse = new() { Id = 3, LengthCm = 45, Label = "Princesse" };
    private readonly ChainLength _miLong = new() { Id = 4, LengthCm = 50, Label = "Mi-long" };

    public ChainLengthCommandHandlerTests() => _lengths.Seed(_princesse, _miLong);

    [Fact]
    public async Task Cree_une_longueur_avec_son_libelle_nettoye()
    {
        var handler = new CreateChainLengthCommandHandler(_lengths);

        var dto = await handler.Handle(new CreateChainLengthCommand(48, "  Princesse longue "), CancellationToken.None);

        var created = Assert.Single(_lengths.All, l => l.LengthCm == 48);
        Assert.Equal("Princesse longue", created.Label);
        Assert.Equal(48, dto.LengthCm);
        Assert.Equal("Princesse longue", dto.Label);
        Assert.Equal(1, _lengths.SaveChangesCount);
    }

    [Fact]
    public async Task Refuse_de_creer_une_longueur_deja_existante()
    {
        var handler = new CreateChainLengthCommandHandler(_lengths);

        var error = await Assert.ThrowsAsync<ConflictException>(
            () => handler.Handle(new CreateChainLengthCommand(45, "Doublon"), CancellationToken.None));

        Assert.Equal(ChainLengthMessages.AlreadyExists, error.Message);
        Assert.Equal(2, _lengths.All.Count);
        Assert.Equal(0, _lengths.SaveChangesCount);
    }

    [Fact]
    public async Task Renvoie_un_conflit_si_deux_creations_simultanees_violent_l_unicite()
    {
        _lengths.SaveChangesError = UniqueViolation();
        var handler = new CreateChainLengthCommandHandler(_lengths);

        var error = await Assert.ThrowsAsync<ConflictException>(
            () => handler.Handle(new CreateChainLengthCommand(48, "Princesse longue"), CancellationToken.None));

        Assert.Equal(ChainLengthMessages.AlreadyExists, error.Message);
    }

    [Fact]
    public async Task Renvoie_un_conflit_si_deux_modifications_simultanees_violent_l_unicite()
    {
        _lengths.SaveChangesError = UniqueViolation();
        var handler = new UpdateChainLengthCommandHandler(_lengths);

        var error = await Assert.ThrowsAsync<ConflictException>(
            () => handler.Handle(new UpdateChainLengthCommand(3, 46, "Princesse"), CancellationToken.None));

        Assert.Equal(ChainLengthMessages.AlreadyExists, error.Message);
    }

    [Fact]
    public async Task Laisse_remonter_les_autres_erreurs_d_enregistrement()
    {
        _lengths.SaveChangesError = new DbUpdateException("Autre erreur.");
        var handler = new CreateChainLengthCommandHandler(_lengths);

        await Assert.ThrowsAsync<DbUpdateException>(
            () => handler.Handle(new CreateChainLengthCommand(48, "Princesse longue"), CancellationToken.None));
    }

    [Fact]
    public async Task Modifie_la_longueur_et_le_libelle()
    {
        var handler = new UpdateChainLengthCommandHandler(_lengths);

        var dto = await handler.Handle(new UpdateChainLengthCommand(3, 46, " Princesse + "), CancellationToken.None);

        Assert.Equal(46, _princesse.LengthCm);
        Assert.Equal("Princesse +", _princesse.Label);
        Assert.Equal(3, dto.Id);
        Assert.Equal(1, _lengths.SaveChangesCount);
    }

    [Fact]
    public async Task Accepte_de_garder_la_meme_longueur_a_la_modification()
    {
        var handler = new UpdateChainLengthCommandHandler(_lengths);

        await handler.Handle(new UpdateChainLengthCommand(3, 45, "Princesse classique"), CancellationToken.None);

        Assert.Equal("Princesse classique", _princesse.Label);
        Assert.Equal(1, _lengths.SaveChangesCount);
    }

    [Fact]
    public async Task Refuse_de_modifier_vers_une_longueur_deja_prise()
    {
        var handler = new UpdateChainLengthCommandHandler(_lengths);

        var error = await Assert.ThrowsAsync<ConflictException>(
            () => handler.Handle(new UpdateChainLengthCommand(3, 50, "Princesse"), CancellationToken.None));

        Assert.Equal(ChainLengthMessages.AlreadyExists, error.Message);
        Assert.Equal(45, _princesse.LengthCm);
        Assert.Equal(0, _lengths.SaveChangesCount);
    }

    [Fact]
    public async Task Refuse_de_modifier_une_longueur_inconnue()
    {
        var handler = new UpdateChainLengthCommandHandler(_lengths);

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(new UpdateChainLengthCommand(99, 46, "Inconnue"), CancellationToken.None));

        Assert.Equal(0, _lengths.SaveChangesCount);
    }

    [Fact]
    public async Task Supprime_une_longueur()
    {
        var handler = new DeleteChainLengthCommandHandler(_lengths);

        await handler.Handle(new DeleteChainLengthCommand(3), CancellationToken.None);

        Assert.DoesNotContain(_princesse, _lengths.All);
        Assert.Equal(1, _lengths.SaveChangesCount);
    }

    [Fact]
    public async Task Refuse_de_supprimer_une_longueur_inconnue()
    {
        var handler = new DeleteChainLengthCommandHandler(_lengths);

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(new DeleteChainLengthCommand(99), CancellationToken.None));

        Assert.Equal(2, _lengths.All.Count);
        Assert.Equal(0, _lengths.SaveChangesCount);
    }
}

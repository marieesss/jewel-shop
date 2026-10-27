using JewelryShop.API.Tests.Fakes;
using JewelryShop.Common.Enums;
using JewelryShop.Common.Exceptions;
using JewelryShop.Features.Charms.Commands;
using JewelryShop.Features.Tags.Entities;
using Microsoft.EntityFrameworkCore;
using static JewelryShop.API.Tests.Tags.TagFixtures;

namespace JewelryShop.API.Tests.Charms;

public class CreateCharmCommandHandlerTests
{
    private readonly FakeCharmRepository _charms = new();
    private readonly FakeTagRepository _tags = new();
    private readonly CreateCharmCommandHandler _handler;

    public CreateCharmCommandHandlerTests() => _handler = new CreateCharmCommandHandler(_charms, _tags);

    private static CreateCharmCommand Command(long[]? tagIds = null) =>
        new("Trèfle", "Porte-bonheur", ProductColor.Silver, Cost: 2m, Price: 8m, Stock: 10, TagIds: tagIds);

    [Fact]
    public async Task Associe_les_tags_demandes_tries_par_categorie_puis_nom()
    {
        _tags.Seed(Tag(1, "Nature"), Tag(2, "Argent", TagCategory.Color), Tag(3, "Chance"));

        var dto = await _handler.Handle(Command([2, 1, 3]), CancellationToken.None);

        var charm = Assert.Single(_charms.All);
        Assert.Equal([1L, 2L, 3L], charm.CharmTags.Select(ct => ct.TagId).Order());
        Assert.Equal(["Chance", "Nature", "Argent"], dto.Tags!.Select(t => t.Name));
    }

    [Fact]
    public async Task Cree_la_breloque_sans_tag_quand_tag_ids_est_absent()
    {
        var dto = await _handler.Handle(Command(), CancellationToken.None);

        Assert.Empty(Assert.Single(_charms.All).CharmTags);
        Assert.Empty(dto.Tags!);
    }

    [Fact]
    public async Task Refuse_un_tag_inconnu_sans_creer_la_breloque()
    {
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _handler.Handle(Command([42]), CancellationToken.None));

        Assert.Equal(["Tag introuvable."], exception.Errors[nameof(CreateCharmCommand.TagIds)]);
        Assert.Empty(_charms.All);
    }

    [Fact]
    public async Task Renvoie_tag_introuvable_si_le_tag_est_supprime_pendant_l_enregistrement_breloque()
    {
        _tags.Seed(Tag(1, "Mer"));
        _charms.SaveChangesError = ForeignKeyViolation();

        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _handler.Handle(Command([1]), CancellationToken.None));

        Assert.Equal(["Tag introuvable."], exception.Errors[nameof(CreateCharmCommand.TagIds)]);
    }

    [Fact]
    public async Task Laisse_remonter_les_autres_erreurs_d_enregistrement_breloque()
    {
        _tags.Seed(Tag(1, "Mer"));
        _charms.SaveChangesError = new DbUpdateException("Autre erreur.");

        await Assert.ThrowsAsync<DbUpdateException>(
            () => _handler.Handle(Command([1]), CancellationToken.None));
    }
}

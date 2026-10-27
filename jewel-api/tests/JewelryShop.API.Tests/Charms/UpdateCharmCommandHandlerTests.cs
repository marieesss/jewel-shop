using JewelryShop.API.Tests.Fakes;
using JewelryShop.Common.Enums;
using JewelryShop.Common.Exceptions;
using JewelryShop.Features.Charms.Commands;
using JewelryShop.Features.Charms.Entities;
using Microsoft.EntityFrameworkCore;
using static JewelryShop.API.Tests.Tags.TagFixtures;

namespace JewelryShop.API.Tests.Charms;

public class UpdateCharmCommandHandlerTests
{
    private const long CharmId = 7;

    private readonly FakeCharmRepository _charms = new();
    private readonly FakeTagRepository _tags = new();
    private readonly UpdateCharmCommandHandler _handler;
    private readonly Charm _charm;

    public UpdateCharmCommandHandlerTests()
    {
        _charm = new Charm { Id = CharmId, Name = "Trèfle", Color = ProductColor.Silver, Cost = 2m, Price = 8m, Stock = 10 };
        _charms.Seed(_charm);
        _tags.Seed(Tag(1, "Nature"), Tag(2, "Chance"), Tag(3, "Mer"));
        _charm.ReplaceTags([_tags.All[2]]);
        _handler = new UpdateCharmCommandHandler(_charms, _tags);
    }

    private static UpdateCharmCommand Command(long[]? tagIds = null) =>
        new(CharmId, "Trèfle", "Porte-bonheur", ProductColor.Silver, Cost: 2m, Price: 9m, Stock: 10, TagIds: tagIds);

    [Fact]
    public async Task Remplace_les_tags_par_ceux_demandes()
    {
        var dto = await _handler.Handle(Command([1, 2]), CancellationToken.None);

        Assert.Equal([1L, 2L], _charm.CharmTags.Select(ct => ct.TagId).Order());
        Assert.Equal(["Chance", "Nature"], dto.Tags!.Select(t => t.Name));
    }

    [Fact]
    public async Task Garde_les_tags_quand_tag_ids_est_absent()
    {
        var dto = await _handler.Handle(Command(), CancellationToken.None);

        Assert.Equal([3L], _charm.CharmTags.Select(ct => ct.TagId));
        Assert.Equal(["Mer"], dto.Tags!.Select(t => t.Name));
        Assert.Equal(9m, _charm.Price);
    }

    [Fact]
    public async Task Retire_tous_les_tags_avec_une_liste_vide()
    {
        var dto = await _handler.Handle(Command([]), CancellationToken.None);

        Assert.Empty(_charm.CharmTags);
        Assert.Empty(dto.Tags!);
    }

    [Fact]
    public async Task Refuse_un_tag_inconnu_sans_enregistrer()
    {
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _handler.Handle(Command([1, 99]), CancellationToken.None));

        Assert.Equal(["Tag introuvable."], exception.Errors[nameof(UpdateCharmCommand.TagIds)]);
        Assert.Equal([3L], _charm.CharmTags.Select(ct => ct.TagId));
        Assert.Equal(0, _charms.SaveChangesCount);
    }

    [Fact]
    public async Task Renvoie_tag_introuvable_si_le_tag_est_supprime_pendant_l_enregistrement_breloque()
    {
        _charms.SaveChangesError = ForeignKeyViolation();

        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _handler.Handle(Command([1]), CancellationToken.None));

        Assert.Equal(["Tag introuvable."], exception.Errors[nameof(UpdateCharmCommand.TagIds)]);
    }

    [Fact]
    public async Task Laisse_remonter_les_autres_erreurs_d_enregistrement_breloque()
    {
        _charms.SaveChangesError = new DbUpdateException("Autre erreur.");

        await Assert.ThrowsAsync<DbUpdateException>(
            () => _handler.Handle(Command([1]), CancellationToken.None));
    }
}

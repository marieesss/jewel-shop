using JewelryShop.API.Tests.Fakes;
using JewelryShop.Common.Exceptions;
using JewelryShop.Features.Tags.Commands;
using JewelryShop.Features.Tags.Common;
using JewelryShop.Features.Tags.Entities;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using static JewelryShop.API.Tests.Tags.TagFixtures;

namespace JewelryShop.API.Tests.Tags;

public class TagCommandHandlerTests
{
    private static DbUpdateException UniqueViolation()
        => new("Violation d'unicité.", new PostgresException("duplicate key", "ERROR", "ERROR", PostgresErrorCodes.UniqueViolation));

    private readonly FakeTagRepository _tags = new();
    private readonly Tag _sea = Tag(1, "Mer", TagCategory.Theme);
    private readonly Tag _goldColor = Tag(2, "Doré", TagCategory.Color);
    private readonly Tag _silver = Tag(3, "Argent", TagCategory.Material);

    public TagCommandHandlerTests() => _tags.Seed(_sea, _goldColor, _silver);

    [Fact]
    public async Task Cree_un_tag_avec_son_nom_nettoye_et_son_slug()
    {
        var handler = new CreateTagCommandHandler(_tags);

        var dto = await handler.Handle(new CreateTagCommand("  Fête des mères ", "Occasion"), CancellationToken.None);

        var created = Assert.Single(_tags.All, t => t.Category == TagCategory.Occasion);
        Assert.Equal("Fête des mères", created.Name);
        Assert.Equal("fete-des-meres", created.Slug);
        Assert.Equal("Fête des mères", dto.Name);
        Assert.Equal("fete-des-meres", dto.Slug);
        Assert.Equal("Occasion", dto.Category);
        Assert.Equal(1, _tags.SaveChangesCount);
    }

    [Fact]
    public async Task Accepte_le_meme_nom_dans_une_autre_categorie()
    {
        var handler = new CreateTagCommandHandler(_tags);

        var dto = await handler.Handle(new CreateTagCommand("Doré", "Material"), CancellationToken.None);

        Assert.Equal("dore", dto.Slug);
        Assert.Equal(2, _tags.All.Count(t => t.Slug == "dore"));
    }

    [Theory]
    [InlineData("Mer")]
    [InlineData("MER")]
    [InlineData(" mer ")]
    public async Task Refuse_de_creer_un_nom_deja_pris_dans_la_categorie(string name)
    {
        var handler = new CreateTagCommandHandler(_tags);

        var error = await Assert.ThrowsAsync<ConflictException>(
            () => handler.Handle(new CreateTagCommand(name, "Theme"), CancellationToken.None));

        Assert.Equal(TagMessages.AlreadyExists(name.Trim()), error.Message);
        Assert.Equal(3, _tags.All.Count);
        Assert.Equal(0, _tags.SaveChangesCount);
    }

    [Fact]
    public async Task Refuse_un_nom_different_qui_donne_le_slug_d_un_tag_de_la_categorie()
    {
        var handler = new CreateTagCommandHandler(_tags);

        var error = await Assert.ThrowsAsync<ConflictException>(
            () => handler.Handle(new CreateTagCommand("Dore", "Color"), CancellationToken.None));

        Assert.Equal(TagMessages.TooCloseTo("Doré"), error.Message);
        Assert.Equal(0, _tags.SaveChangesCount);
    }

    [Fact]
    public async Task Renvoie_un_conflit_si_deux_creations_simultanees_violent_l_unicite()
    {
        _tags.SaveChangesError = UniqueViolation();
        var handler = new CreateTagCommandHandler(_tags);

        var error = await Assert.ThrowsAsync<ConflictException>(
            () => handler.Handle(new CreateTagCommand(" Plage ", "Theme"), CancellationToken.None));

        Assert.Equal(TagMessages.AlreadyExists("Plage"), error.Message);
    }

    [Fact]
    public async Task Laisse_remonter_les_autres_erreurs_d_enregistrement()
    {
        _tags.SaveChangesError = new DbUpdateException("Autre erreur.");
        var handler = new CreateTagCommandHandler(_tags);

        await Assert.ThrowsAsync<DbUpdateException>(
            () => handler.Handle(new CreateTagCommand("Plage", "Theme"), CancellationToken.None));
    }

    [Fact]
    public async Task Renomme_un_tag_et_recalcule_son_slug_sans_changer_sa_categorie()
    {
        var handler = new RenameTagCommandHandler(_tags);

        var dto = await handler.Handle(new RenameTagCommand(1, " Océan "), CancellationToken.None);

        Assert.Equal("Océan", _sea.Name);
        Assert.Equal("ocean", _sea.Slug);
        Assert.Equal(TagCategory.Theme, _sea.Category);
        Assert.Equal(1, dto.Id);
        Assert.Equal("ocean", dto.Slug);
        Assert.Equal(1, _tags.SaveChangesCount);
    }

    [Fact]
    public async Task Accepte_de_corriger_seulement_la_casse_ou_les_accents_du_nom()
    {
        var handler = new RenameTagCommandHandler(_tags);

        await handler.Handle(new RenameTagCommand(2, "doré"), CancellationToken.None);
        await handler.Handle(new RenameTagCommand(2, "Dore"), CancellationToken.None);

        Assert.Equal("Dore", _goldColor.Name);
        Assert.Equal("dore", _goldColor.Slug);
        Assert.Equal(2, _tags.SaveChangesCount);
    }

    [Fact]
    public async Task Refuse_de_renommer_vers_un_nom_deja_pris_dans_la_categorie()
    {
        _tags.Seed(Tag(4, "Plage", TagCategory.Theme));
        var handler = new RenameTagCommandHandler(_tags);

        var error = await Assert.ThrowsAsync<ConflictException>(
            () => handler.Handle(new RenameTagCommand(1, "plage"), CancellationToken.None));

        Assert.Equal(TagMessages.AlreadyExists("plage"), error.Message);
        Assert.Equal("Mer", _sea.Name);
        Assert.Equal(0, _tags.SaveChangesCount);
    }

    [Fact]
    public async Task Refuse_de_renommer_vers_un_nom_trop_proche_d_un_tag_de_la_categorie()
    {
        _tags.Seed(Tag(4, "Été", TagCategory.Theme));
        var handler = new RenameTagCommandHandler(_tags);

        var error = await Assert.ThrowsAsync<ConflictException>(
            () => handler.Handle(new RenameTagCommand(1, "Ete"), CancellationToken.None));

        Assert.Equal(TagMessages.TooCloseTo("Été"), error.Message);
        Assert.Equal("mer", _sea.Slug);
        Assert.Equal(0, _tags.SaveChangesCount);
    }

    [Fact]
    public async Task Accepte_de_renommer_vers_un_nom_pris_dans_une_autre_categorie()
    {
        var handler = new RenameTagCommandHandler(_tags);

        await handler.Handle(new RenameTagCommand(3, "Doré"), CancellationToken.None);

        Assert.Equal("dore", _silver.Slug);
    }

    [Fact]
    public async Task Refuse_de_renommer_un_tag_inconnu()
    {
        var handler = new RenameTagCommandHandler(_tags);

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(new RenameTagCommand(99, "Océan"), CancellationToken.None));

        Assert.Equal(0, _tags.SaveChangesCount);
    }

    [Fact]
    public async Task Renvoie_un_conflit_si_deux_renommages_simultanes_violent_l_unicite()
    {
        _tags.SaveChangesError = UniqueViolation();
        var handler = new RenameTagCommandHandler(_tags);

        var error = await Assert.ThrowsAsync<ConflictException>(
            () => handler.Handle(new RenameTagCommand(1, "Océan"), CancellationToken.None));

        Assert.Equal(TagMessages.AlreadyExists("Océan"), error.Message);
    }

    [Fact]
    public async Task Supprime_un_tag()
    {
        var handler = new DeleteTagCommandHandler(_tags);

        await handler.Handle(new DeleteTagCommand(1), CancellationToken.None);

        Assert.DoesNotContain(_sea, _tags.All);
        Assert.Equal(1, _tags.SaveChangesCount);
    }

    [Fact]
    public async Task Refuse_de_supprimer_un_tag_inconnu()
    {
        var handler = new DeleteTagCommandHandler(_tags);

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(new DeleteTagCommand(99), CancellationToken.None));

        Assert.Equal(3, _tags.All.Count);
        Assert.Equal(0, _tags.SaveChangesCount);
    }
}

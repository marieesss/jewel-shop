using JewelryShop.API.Tests.Fakes;
using JewelryShop.Features.Invitations.Entities;
using JewelryShop.Features.Invitations.Queries;

namespace JewelryShop.API.Tests.Invitations;

public class GetInvitationsQueryHandlerTests
{
    private static readonly DateTimeOffset _now = new(2026, 10, 15, 8, 0, 0, TimeSpan.Zero);

    private readonly FakeInvitationRepository _invitations = new();
    private readonly GetInvitationsQueryHandler _handler;

    public GetInvitationsQueryHandlerTests()
    {
        _handler = new GetInvitationsQueryHandler(_invitations, new FakeTimeProvider(_now));
        var now = _now.UtcDateTime;
        _invitations.Seed(
            InvitationTestData.Create(1, "ancienne@exemple.com", now.AddDays(-5)),
            InvitationTestData.Create(2, "recente@exemple.com", now.AddDays(-1)),
            InvitationTestData.Create(3, "depassee@exemple.com", now.AddDays(-8)),
            InvitationTestData.Create(4, "expiree@exemple.com", now.AddDays(-9), InvitationStatus.Expired),
            InvitationTestData.Create(5, "revoquee@exemple.com", now.AddDays(-2), InvitationStatus.Revoked, now.AddDays(-1)),
            InvitationTestData.Create(6, "acceptee@exemple.com", now.AddDays(-3), InvitationStatus.Accepted));
    }

    [Fact]
    public async Task Sans_parametre_renvoie_les_invitations_en_attente_non_expirees_de_la_plus_recente_a_la_plus_ancienne()
    {
        var result = await _handler.Handle(new GetInvitationsQuery(null), CancellationToken.None);

        Assert.Equal(new long[] { 2, 1 }, result.Items.Select(i => i.Id));
        Assert.All(result.Items, i => Assert.Equal("Pending", i.Status));
        Assert.Equal(2, result.TotalCount);
    }

    [Fact]
    public async Task Renvoie_une_invitation_en_attente_depassee_avec_les_expirees()
    {
        var result = await _handler.Handle(new GetInvitationsQuery("Expired"), CancellationToken.None);

        Assert.Equal(new long[] { 3, 4 }, result.Items.Select(i => i.Id));
        Assert.All(result.Items, i => Assert.Equal("Expired", i.Status));
    }

    [Theory]
    [InlineData("Revoked", 5)]
    [InlineData("accepted", 6)]
    public async Task Filtre_sur_le_statut_demande(string status, long expectedId)
    {
        var result = await _handler.Handle(new GetInvitationsQuery(status), CancellationToken.None);

        Assert.Equal(expectedId, Assert.Single(result.Items).Id);
    }

    [Fact]
    public async Task Pagine_le_resultat()
    {
        var result = await _handler.Handle(new GetInvitationsQuery(null, Page: 2, PageSize: 1), CancellationToken.None);

        Assert.Equal(1, Assert.Single(result.Items).Id);
        Assert.Equal(2, result.Page);
        Assert.Equal(1, result.PageSize);
        Assert.Equal(2, result.TotalCount);
    }

    [Fact]
    public async Task Normalise_une_pagination_hors_bornes()
    {
        var result = await _handler.Handle(new GetInvitationsQuery(null, Page: 0, PageSize: 500), CancellationToken.None);

        Assert.Equal(1, result.Page);
        Assert.Equal(100, result.PageSize);
    }

    [Fact]
    public async Task Renvoie_le_nom_de_l_admin_qui_a_invite()
    {
        var result = await _handler.Handle(new GetInvitationsQuery(null), CancellationToken.None);

        Assert.All(result.Items, i => Assert.Equal("Marie Espinosa", i.InvitedBy));
    }
}

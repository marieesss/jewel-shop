using JewelryShop.Features.Invitations.Common;
using JewelryShop.Features.Invitations.Entities;

namespace JewelryShop.API.Tests.Invitations;

public class InvitationQueriesTests
{
    private static readonly DateTime _now = new(2026, 10, 15, 8, 0, 0, DateTimeKind.Utc);

    // Une invitation par cas : l'id désigne le cas dans les données attendues.
    private static readonly Invitation[] _invitations =
    {
        InvitationTestData.Create(1, "valable@exemple.com", _now.AddDays(-6)),
        InvitationTestData.Create(2, "echeance-atteinte@exemple.com", _now.AddDays(-7)),
        InvitationTestData.Create(3, "expiree@exemple.com", _now.AddDays(-10), InvitationStatus.Expired),
        InvitationTestData.Create(4, "revoquee@exemple.com", _now.AddDays(-2), InvitationStatus.Revoked, _now.AddDays(-1)),
        InvitationTestData.Create(5, "acceptee@exemple.com", _now.AddDays(-3), InvitationStatus.Accepted),
        InvitationTestData.Create(6, "revoquee-depassee@exemple.com", _now.AddDays(-20), InvitationStatus.Revoked, _now.AddDays(-19))
    };

    [Theory]
    [InlineData(InvitationStatus.Pending, new long[] { 1 })]
    [InlineData(InvitationStatus.Expired, new long[] { 2, 3 })]
    [InlineData(InvitationStatus.Revoked, new long[] { 4, 6 })]
    [InlineData(InvitationStatus.Accepted, new long[] { 5 })]
    public void Filtre_sur_le_statut_effectif(InvitationStatus status, long[] expectedIds)
    {
        var ids = _invitations.AsQueryable().WhereEffectiveStatus(status, _now).Select(i => i.Id).Order();

        Assert.Equal(expectedIds, ids);
    }

    [Fact]
    public void Trie_de_la_plus_recente_a_la_plus_ancienne_puis_par_id_decroissant()
    {
        var invitations = new[]
        {
            InvitationTestData.Create(1, "a@exemple.com", _now.AddDays(-2)),
            InvitationTestData.Create(2, "b@exemple.com", _now),
            InvitationTestData.Create(3, "c@exemple.com", _now)
        };

        var ids = invitations.AsQueryable().OrderByNewest().Select(i => i.Id);

        Assert.Equal(new long[] { 3, 2, 1 }, ids);
    }

    [Theory]
    [InlineData(null, InvitationStatus.Pending)]
    [InlineData("", InvitationStatus.Pending)]
    [InlineData("Pending", InvitationStatus.Pending)]
    [InlineData("expired", InvitationStatus.Expired)]
    [InlineData("REVOKED", InvitationStatus.Revoked)]
    [InlineData("Accepted", InvitationStatus.Accepted)]
    public void Lit_le_statut_demande_pending_par_defaut_sans_tenir_compte_de_la_casse(string? value, InvitationStatus expected)
    {
        Assert.True(InvitationQueries.TryParseStatus(value, out var status));
        Assert.Equal(expected, status);
    }

    [Theory]
    [InlineData("Inconnu")]
    [InlineData("1")]
    public void Refuse_un_statut_inconnu_ou_numerique(string value)
    {
        Assert.False(InvitationQueries.TryParseStatus(value, out _));
    }
}

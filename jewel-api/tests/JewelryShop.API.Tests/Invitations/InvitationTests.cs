using JewelryShop.Features.Invitations.Entities;
using JewelryShop.Features.Users.Entities;

namespace JewelryShop.API.Tests.Invitations;

public class InvitationTests
{
    private static readonly DateTime _now = new(2026, 10, 15, 8, 0, 0, DateTimeKind.Utc);

    private static readonly User _admin = new() { Id = 1, Name = "Marie", Surname = "Espinosa" };

    private static Invitation CreatePending() => Invitation.Create("nouvelle@exemple.com", new string('a', 64), _admin, _now);

    [Fact]
    public void Est_en_attente_et_expire_7_jours_apres_sa_creation()
    {
        var invitation = CreatePending();

        Assert.Equal(InvitationStatus.Pending, invitation.Status);
        Assert.Equal(_now.AddDays(7), invitation.ExpiresAt);
        Assert.Equal(1, invitation.InvitedById);
    }

    [Fact]
    public void N_est_pas_expiree_avant_son_echeance()
    {
        var invitation = CreatePending();

        Assert.False(invitation.IsExpired(invitation.ExpiresAt.AddSeconds(-1)));
        Assert.True(invitation.IsAwaitingAcceptance(invitation.ExpiresAt.AddSeconds(-1)));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void Est_expiree_a_son_echeance_et_apres_meme_si_son_statut_est_encore_en_attente(int secondsAfterExpiry)
    {
        var invitation = CreatePending();
        var now = invitation.ExpiresAt.AddSeconds(secondsAfterExpiry);

        Assert.True(invitation.IsExpired(now));
        Assert.False(invitation.IsAwaitingAcceptance(now));
        Assert.Equal(InvitationStatus.Expired, invitation.EffectiveStatus(now));
    }

    [Fact]
    public void Est_expiree_quand_son_statut_est_expired_meme_avant_son_echeance()
    {
        var invitation = CreatePending();

        invitation.MarkExpired();

        Assert.True(invitation.IsExpired(_now));
        Assert.Equal(InvitationStatus.Expired, invitation.EffectiveStatus(_now));
    }

    [Fact]
    public void Donne_le_prenom_et_le_nom_de_l_admin()
    {
        Assert.Equal("Marie Espinosa", CreatePending().InviterName);
    }

    [Fact]
    public void N_a_pas_de_nom_d_admin_si_l_admin_n_a_ni_prenom_ni_nom()
    {
        var invitation = Invitation.Create("nouvelle@exemple.com", new string('a', 64), new User { Id = 1, Name = "", Surname = "" }, _now);

        Assert.Null(invitation.InviterName);
    }

    [Fact]
    public void Renouvelee_repasse_en_attente_avec_un_nouveau_token_valable_7_jours()
    {
        var invitation = InvitationTestData.Create(1, "nouvelle@exemple.com", _now.AddDays(-20), InvitationStatus.Revoked, _now.AddDays(-19));

        invitation.Renew(new string('b', 64), _now);

        Assert.Equal(InvitationStatus.Pending, invitation.Status);
        Assert.Equal(new string('b', 64), invitation.TokenHash);
        Assert.Equal(_now.AddDays(7), invitation.ExpiresAt);
        Assert.Null(invitation.RevokedAt);
        Assert.Equal(_now.AddDays(-20), invitation.CreatedAt);
    }
}

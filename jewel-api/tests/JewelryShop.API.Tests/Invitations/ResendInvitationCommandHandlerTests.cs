using JewelryShop.API.Tests.Fakes;
using JewelryShop.Common;
using JewelryShop.Common.Exceptions;
using JewelryShop.Features.Invitations.Commands;
using JewelryShop.Features.Invitations.Entities;
using JewelryShop.Features.Users.Entities;
using JewelryShop.Infrastructure.Email;
using JewelryShop.Infrastructure.Services;
using Microsoft.Extensions.Options;

namespace JewelryShop.API.Tests.Invitations;

public class ResendInvitationCommandHandlerTests
{
    private const long InvitationId = 10;
    private const string Email = "nouvelle@exemple.com";
    private static readonly string _oldTokenHash = new('a', 64);

    private static readonly DateTimeOffset _now = new(2026, 10, 15, 8, 0, 0, TimeSpan.Zero);
    private static readonly DateTime _createdAt = _now.UtcDateTime.AddDays(-10);

    private readonly FakeInvitationRepository _invitations = new();
    private readonly FakeUserRepository _users = new();
    private readonly FakeEmailOutboxRepository _outbox = new();
    private readonly SecureTokenGenerator _tokenGenerator = new();
    private readonly ResendInvitationCommandHandler _handler;

    public ResendInvitationCommandHandlerTests()
    {
        var timeProvider = new FakeTimeProvider(_now);
        var emailQueue = new EmailQueue(new ScribanEmailTemplateRenderer(), _outbox, timeProvider);
        _handler = new ResendInvitationCommandHandler(
            _invitations, _users, _tokenGenerator, emailQueue, Options.Create(new AppOptions()), timeProvider);
    }

    [Fact]
    public async Task Remet_une_invitation_expiree_en_attente_a_J_plus_7_avec_un_nouveau_lien()
    {
        var invitation = Seed(InvitationStatus.Expired);

        var result = await _handler.Handle(new ResendInvitationCommand(InvitationId), CancellationToken.None);

        Assert.Equal(InvitationStatus.Pending, invitation.Status);
        Assert.Equal(_now.UtcDateTime.AddDays(7), invitation.ExpiresAt);
        Assert.NotEqual(_oldTokenHash, invitation.TokenHash);
        Assert.Equal("Pending", result.Status);
        Assert.Equal(_now.UtcDateTime.AddDays(7), result.ExpiresAt);
    }

    [Fact]
    public async Task Met_en_file_un_nouveau_mail_avec_le_lien_du_nouveau_token()
    {
        var invitation = Seed(InvitationStatus.Expired);

        await _handler.Handle(new ResendInvitationCommand(InvitationId), CancellationToken.None);

        var email = Assert.Single(_outbox.All);
        Assert.Equal(Email, email.ToAddress);
        Assert.Contains("Marie Espinosa vous invite", email.TextBody);
        Assert.Contains("valable jusqu'au 22/10/2026", email.TextBody);
        Assert.Equal(invitation.TokenHash, _tokenGenerator.Hash(ExtractToken(email.TextBody)));
    }

    [Fact]
    public async Task Remet_une_invitation_revoquee_en_attente_et_vide_sa_date_de_revocation()
    {
        var invitation = Seed(InvitationStatus.Revoked, revokedAt: _now.UtcDateTime.AddDays(-1));

        await _handler.Handle(new ResendInvitationCommand(InvitationId), CancellationToken.None);

        Assert.Equal(InvitationStatus.Pending, invitation.Status);
        Assert.Null(invitation.RevokedAt);
    }

    [Fact]
    public async Task Renvoie_une_invitation_encore_en_attente_avec_un_nouveau_token()
    {
        var invitation = Seed(InvitationStatus.Pending, createdAt: _now.UtcDateTime.AddDays(-2));

        await _handler.Handle(new ResendInvitationCommand(InvitationId), CancellationToken.None);

        Assert.Equal(InvitationStatus.Pending, invitation.Status);
        Assert.Equal(_now.UtcDateTime.AddDays(7), invitation.ExpiresAt);
        Assert.NotEqual(_oldTokenHash, invitation.TokenHash);
    }

    [Fact]
    public async Task Garde_l_admin_qui_a_invite_et_la_date_de_creation()
    {
        var invitation = Seed(InvitationStatus.Expired);

        var result = await _handler.Handle(new ResendInvitationCommand(InvitationId), CancellationToken.None);

        Assert.Equal(InvitationTestData.Admin.Id, invitation.InvitedById);
        Assert.Equal(_createdAt, invitation.CreatedAt);
        Assert.Equal(_createdAt, result.CreatedAt);
        Assert.Equal("Marie Espinosa", result.InvitedBy);
    }

    [Fact]
    public async Task Enregistre_l_invitation_et_le_mail_dans_une_seule_sauvegarde()
    {
        Seed(InvitationStatus.Expired);

        await _handler.Handle(new ResendInvitationCommand(InvitationId), CancellationToken.None);

        Assert.Equal(1, _invitations.SaveChangesCount);
        Assert.Equal(0, _outbox.SaveChangesCount);
    }

    [Fact]
    public async Task Refuse_une_invitation_acceptee()
    {
        Seed(InvitationStatus.Accepted);

        var exception = await Assert.ThrowsAsync<ConflictException>(
            () => _handler.Handle(new ResendInvitationCommand(InvitationId), CancellationToken.None));

        Assert.Equal("Cette invitation a déjà été acceptée.", exception.Message);
        AssertNothingSaved();
    }

    [Fact]
    public async Task Refuse_quand_un_compte_existe_desormais_avec_cet_email()
    {
        Seed(InvitationStatus.Expired);
        _users.Seed(new User { Id = 2, Name = "Léa", Surname = "Martin", Email = Email });

        var exception = await Assert.ThrowsAsync<ConflictException>(
            () => _handler.Handle(new ResendInvitationCommand(InvitationId), CancellationToken.None));

        Assert.Equal("Un compte existe déjà avec cet email.", exception.Message);
        AssertNothingSaved();
    }

    [Fact]
    public async Task Refuse_quand_une_autre_invitation_est_en_attente_pour_cet_email()
    {
        var invitation = Seed(InvitationStatus.Revoked, revokedAt: _now.UtcDateTime.AddDays(-1));
        _invitations.Seed(InvitationTestData.Create(11, Email, _now.UtcDateTime.AddDays(-1)));

        var exception = await Assert.ThrowsAsync<ConflictException>(
            () => _handler.Handle(new ResendInvitationCommand(InvitationId), CancellationToken.None));

        Assert.Equal("Une invitation est déjà en attente pour cet email.", exception.Message);
        Assert.Equal(InvitationStatus.Revoked, invitation.Status);
        AssertNothingSaved();
    }

    [Fact]
    public async Task Expire_une_autre_invitation_en_attente_depassee_pour_cet_email()
    {
        Seed(InvitationStatus.Revoked, revokedAt: _now.UtcDateTime.AddDays(-1));
        var outdated = InvitationTestData.Create(11, Email, _now.UtcDateTime.AddDays(-8));
        _invitations.Seed(outdated);

        await _handler.Handle(new ResendInvitationCommand(InvitationId), CancellationToken.None);

        Assert.Equal(InvitationStatus.Expired, outdated.Status);
        Assert.Equal(1, _invitations.SaveChangesCount);
    }

    [Fact]
    public async Task Refuse_une_invitation_inexistante()
    {
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _handler.Handle(new ResendInvitationCommand(99), CancellationToken.None));

        Assert.Equal("Invitation introuvable.", exception.Message);
        AssertNothingSaved();
    }

    private Invitation Seed(InvitationStatus status, DateTime? createdAt = null, DateTime? revokedAt = null)
    {
        var invitation = InvitationTestData.Create(InvitationId, Email, createdAt ?? _createdAt, status, revokedAt);
        _invitations.Seed(invitation);
        return invitation;
    }

    private void AssertNothingSaved()
    {
        Assert.Empty(_outbox.All);
        Assert.Equal(0, _invitations.SaveChangesCount);
    }

    private static string ExtractToken(string body)
    {
        const string prefix = "http://localhost:5173/register?token=";
        var start = body.IndexOf(prefix, StringComparison.Ordinal);
        Assert.True(start >= 0, "Le lien d'inscription est absent du mail.");
        return new string(body[(start + prefix.Length)..].TakeWhile(c => !char.IsWhiteSpace(c)).ToArray());
    }
}

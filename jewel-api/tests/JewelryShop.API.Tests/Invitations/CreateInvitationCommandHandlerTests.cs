using JewelryShop.API.Tests.Fakes;
using JewelryShop.Common;
using JewelryShop.Common.Enums;
using JewelryShop.Common.Exceptions;
using JewelryShop.Features.Emails.Entities;
using JewelryShop.Features.Invitations.Commands;
using JewelryShop.Features.Invitations.Entities;
using JewelryShop.Features.Users.Entities;
using JewelryShop.Infrastructure.Email;
using JewelryShop.Infrastructure.Services;
using Microsoft.Extensions.Options;

namespace JewelryShop.API.Tests.Invitations;

public class CreateInvitationCommandHandlerTests
{
    private const long AdminId = 1;
    private const string Email = "nouvelle@exemple.com";

    private static readonly DateTimeOffset _now = new(2026, 10, 15, 8, 0, 0, TimeSpan.Zero);

    private readonly FakeInvitationRepository _invitations = new();
    private readonly FakeUserRepository _users = new();
    private readonly FakeEmailOutboxRepository _outbox = new();
    private readonly SecureTokenGenerator _tokenGenerator = new();
    private readonly CreateInvitationCommandHandler _handler;

    public CreateInvitationCommandHandlerTests()
    {
        var timeProvider = new FakeTimeProvider(_now);
        var emailQueue = new EmailQueue(new ScribanEmailTemplateRenderer(), _outbox, timeProvider);
        _handler = new CreateInvitationCommandHandler(
            _invitations, _users, _tokenGenerator, emailQueue, Options.Create(new AppOptions()), timeProvider);
        _users.Seed(new User { Id = AdminId, Name = "Marie", Surname = "Espinosa", Email = "marie@mycharmly.eu", Role = UserRole.Admin });
    }

    [Fact]
    public async Task Cree_une_invitation_en_attente_valable_7_jours_avec_un_token_hache()
    {
        await _handler.Handle(new CreateInvitationCommand(Email, AdminId), CancellationToken.None);

        var invitation = Assert.Single(_invitations.All);
        Assert.Equal(Email, invitation.Email);
        Assert.Equal(InvitationStatus.Pending, invitation.Status);
        Assert.Equal(_now.UtcDateTime.AddDays(7), invitation.ExpiresAt);
        Assert.Equal(AdminId, invitation.InvitedById);
        Assert.Equal(64, invitation.TokenHash.Length);
    }

    [Fact]
    public async Task Renvoie_l_invitation_avec_le_nom_de_l_admin()
    {
        var result = await _handler.Handle(new CreateInvitationCommand(Email, AdminId), CancellationToken.None);

        Assert.Equal(Email, result.Email);
        Assert.Equal("Pending", result.Status);
        Assert.Equal(_now.UtcDateTime.AddDays(7), result.ExpiresAt);
        Assert.Equal(_now.UtcDateTime, result.CreatedAt);
        Assert.Equal("Marie Espinosa", result.InvitedBy);
    }

    [Fact]
    public async Task Normalise_l_email()
    {
        await _handler.Handle(new CreateInvitationCommand("  Nouvelle@Exemple.COM ", AdminId), CancellationToken.None);

        Assert.Equal(Email, _invitations.All.Single().Email);
        Assert.Equal(Email, _outbox.All.Single().ToAddress);
    }

    [Fact]
    public async Task Met_en_file_le_mail_avec_le_lien_d_inscription_du_token()
    {
        await _handler.Handle(new CreateInvitationCommand(Email, AdminId), CancellationToken.None);

        var email = Assert.Single(_outbox.All);
        Assert.Equal(Email, email.ToAddress);
        Assert.Equal("Votre invitation à rejoindre mycharmly", email.Subject);
        Assert.Contains("Marie Espinosa vous invite", email.TextBody);
        Assert.Contains("valable jusqu'au 22/10/2026", email.TextBody);
        var token = ExtractToken(email.TextBody);
        Assert.Equal(_invitations.All.Single().TokenHash, _tokenGenerator.Hash(token));
    }

    [Fact]
    public async Task Enregistre_l_invitation_et_le_mail_dans_une_seule_sauvegarde()
    {
        await _handler.Handle(new CreateInvitationCommand(Email, AdminId), CancellationToken.None);

        Assert.Equal(1, _invitations.SaveChangesCount);
        Assert.Equal(0, _users.SaveChangesCount);
        Assert.Equal(0, _outbox.SaveChangesCount);
    }

    [Fact]
    public async Task Laisse_le_mail_en_attente_dans_l_outbox_sans_dependre_du_fournisseur_de_mail()
    {
        await _handler.Handle(new CreateInvitationCommand(Email, AdminId), CancellationToken.None);

        Assert.Equal(EmailOutboxStatus.Pending, _outbox.All.Single().Status);
    }

    [Fact]
    public async Task Refuse_un_admin_introuvable()
    {
        await Assert.ThrowsAsync<NotFoundException>(
            () => _handler.Handle(new CreateInvitationCommand(Email, 99), CancellationToken.None));

        Assert.Empty(_invitations.All);
        Assert.Empty(_outbox.All);
    }

    [Fact]
    public async Task Refuse_un_email_deja_inscrit()
    {
        _users.Seed(new User { Id = 2, Name = "Léa", Surname = "Martin", Email = Email });

        var exception = await Assert.ThrowsAsync<ConflictException>(
            () => _handler.Handle(new CreateInvitationCommand(Email, AdminId), CancellationToken.None));

        Assert.Equal("Un compte existe déjà avec cet email.", exception.Message);
        Assert.Empty(_invitations.All);
        Assert.Empty(_outbox.All);
    }

    [Fact]
    public async Task Refuse_un_email_qui_a_deja_une_invitation_en_attente_non_expiree()
    {
        var pending = SeedPendingInvitationCreatedAt(_now.UtcDateTime.AddDays(-6));

        var exception = await Assert.ThrowsAsync<ConflictException>(
            () => _handler.Handle(new CreateInvitationCommand(Email, AdminId), CancellationToken.None));

        Assert.Equal("Une invitation est déjà en attente pour cet email.", exception.Message);
        Assert.Equal(InvitationStatus.Pending, pending.Status);
        Assert.Single(_invitations.All);
        Assert.Empty(_outbox.All);
        Assert.Equal(0, _invitations.SaveChangesCount);
    }

    [Fact]
    public async Task Expire_l_invitation_en_attente_depassee_et_en_cree_une_nouvelle()
    {
        var outdated = SeedPendingInvitationCreatedAt(_now.UtcDateTime.AddDays(-7));

        await _handler.Handle(new CreateInvitationCommand(Email, AdminId), CancellationToken.None);

        Assert.Equal(InvitationStatus.Expired, outdated.Status);
        Assert.Equal(2, _invitations.All.Count);
        Assert.Single(_invitations.All, i => i.Status == InvitationStatus.Pending);
        Assert.Equal(1, _invitations.SaveChangesCount);
    }

    private Invitation SeedPendingInvitationCreatedAt(DateTime createdAt)
    {
        var invitation = Invitation.Create(Email, new string('a', 64), _users.All.Single(), createdAt);
        _invitations.Seed(invitation);
        return invitation;
    }

    private static string ExtractToken(string body)
    {
        const string prefix = "http://localhost:5173/register?token=";
        var start = body.IndexOf(prefix, StringComparison.Ordinal);
        Assert.True(start >= 0, "Le lien d'inscription est absent du mail.");
        return new string(body[(start + prefix.Length)..].TakeWhile(c => !char.IsWhiteSpace(c)).ToArray());
    }
}

using JewelryShop.API.Tests.Fakes;
using JewelryShop.Common.Enums;
using JewelryShop.Common.Exceptions;
using JewelryShop.Features.Auth.Commands;
using JewelryShop.Features.Auth.Entities;
using JewelryShop.Features.Invitations.Entities;
using JewelryShop.Features.Users.Entities;
using JewelryShop.Infrastructure.Services;

namespace JewelryShop.API.Tests.Auth;

public class RegisterCommandHandlerTests
{
    private static readonly DateTimeOffset _now = new(2026, 10, 15, 8, 0, 0, TimeSpan.Zero);

    private const string InvitationToken = "token-de-l-invitation";
    private const string InvitationEmail = "invitee@exemple.com";
    private const string InvalidInvitationMessage = "Cette invitation est invalide ou a expiré.";

    private static readonly User _admin = new() { Id = 1, Name = "Marie", Surname = "Espinosa" };

    private readonly FakeUserRepository _users = new();
    private readonly FakeInvitationRepository _invitations = new();
    private readonly FakeRefreshTokenRepository _refreshTokens = new();
    private readonly SecureTokenGenerator _tokenGenerator = new();
    private readonly RegisterCommandHandler _handler;

    public RegisterCommandHandlerTests() => _handler = CreateHandler(_now);

    private RegisterCommandHandler CreateHandler(DateTimeOffset now)
    {
        var timeProvider = new FakeTimeProvider(now);
        var refreshTokenService = new RefreshTokenService(
            _refreshTokens, _tokenGenerator, timeProvider, new ListLogger<RefreshTokenService>());
        return new RegisterCommandHandler(
            _users,
            _invitations,
            new FakePasswordHasher(),
            new FakeJwtService(),
            _tokenGenerator,
            timeProvider,
            refreshTokenService);
    }

    private static RegisterCommand ValidCommand() =>
        new("Marie", "Dupont", "Marie@MyCharmly.eu", "Secret123!", new DateOnly(1990, 5, 12));

    private static RegisterCommand InvitedCommand(string? email = "autre@exemple.com") =>
        ValidCommand() with { Email = email, InvitationToken = InvitationToken };

    private Invitation SeedInvitation(InvitationStatus status = InvitationStatus.Pending)
    {
        var invitation = Invitation.Create(
            InvitationEmail, _tokenGenerator.Hash(InvitationToken), _admin, _now.UtcDateTime.AddDays(-1));
        typeof(Invitation).GetProperty(nameof(Invitation.Status))!.SetValue(invitation, status);
        _invitations.Seed(invitation);
        return invitation;
    }

    [Fact]
    public async Task Cree_l_utilisateur_avec_le_role_user()
    {
        var result = await _handler.Handle(ValidCommand(), CancellationToken.None);

        var user = Assert.Single(_users.All);
        Assert.Equal("marie@mycharmly.eu", user.Email);
        Assert.Equal(UserRole.User, user.Role);
        Assert.Equal("User", result.Response.Role);
    }

    [Fact]
    public async Task Enregistre_la_date_et_l_heure_utc_de_l_inscription()
    {
        await _handler.Handle(ValidCommand(), CancellationToken.None);

        var user = Assert.Single(_users.All);
        Assert.Equal(_now.UtcDateTime, user.CreatedAt);
        Assert.Equal(DateTimeKind.Utc, user.CreatedAt!.Value.Kind);
    }

    [Fact]
    public async Task Emet_le_refresh_token_dans_la_meme_sauvegarde_que_l_utilisateur()
    {
        var result = await _handler.Handle(ValidCommand(), CancellationToken.None);

        var stored = Assert.Single(_refreshTokens.All);
        Assert.Same(_users.All.Single(), stored.User);
        Assert.Equal(_tokenGenerator.Hash(result.RefreshToken), stored.TokenHash);
        Assert.Equal(1, _users.SaveChangesCount);
        Assert.Equal(0, _refreshTokens.SaveChangesCount);
    }

    [Fact]
    public async Task Refuse_un_email_deja_utilise_sans_emettre_de_refresh_token()
    {
        _users.Seed(new User { Id = 1, Email = "marie@mycharmly.eu" });

        var exception = await Assert.ThrowsAsync<ConflictException>(
            () => _handler.Handle(ValidCommand(), CancellationToken.None));

        Assert.Equal("Un compte existe déjà avec cet email.", exception.Message);
        Assert.Empty(_refreshTokens.All);
        Assert.Equal(0, _users.SaveChangesCount);
    }

    [Fact]
    public async Task Avec_une_invitation_cree_le_compte_avec_l_email_de_l_invitation_et_non_celui_du_corps()
    {
        SeedInvitation();

        var result = await _handler.Handle(InvitedCommand(), CancellationToken.None);

        var user = Assert.Single(_users.All);
        Assert.Equal(InvitationEmail, user.Email);
        Assert.Equal("Marie", user.Name);
        Assert.Equal("Dupont", user.Surname);
        Assert.Equal(new DateOnly(1990, 5, 12), user.Birthday);
        Assert.Equal(UserRole.User, user.Role);
        Assert.Equal("User", result.Response.Role);
    }

    [Fact]
    public async Task Avec_une_invitation_l_accepte_dans_la_meme_sauvegarde_que_le_compte_et_le_refresh_token()
    {
        var invitation = SeedInvitation();

        await _handler.Handle(InvitedCommand(), CancellationToken.None);

        Assert.Equal(InvitationStatus.Accepted, invitation.Status);
        Assert.Equal(_now.UtcDateTime, invitation.AcceptedAt);
        Assert.Same(_users.All.Single(), invitation.AcceptedUser);
        Assert.Single(_refreshTokens.All);
        Assert.Equal(1, _users.SaveChangesCount);
        Assert.Equal(0, _invitations.SaveChangesCount);
        Assert.Equal(0, _refreshTokens.SaveChangesCount);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Avec_une_invitation_accepte_un_email_vide_ou_absent(string? email)
    {
        SeedInvitation();

        await _handler.Handle(InvitedCommand(email), CancellationToken.None);

        Assert.Equal(InvitationEmail, Assert.Single(_users.All).Email);
    }

    [Theory]
    [InlineData(InvitationStatus.Accepted)]
    [InlineData(InvitationStatus.Expired)]
    [InlineData(InvitationStatus.Revoked)]
    public async Task Refuse_une_invitation_qui_n_est_plus_en_attente(InvitationStatus status)
    {
        SeedInvitation(status);

        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _handler.Handle(InvitedCommand(), CancellationToken.None));

        Assert.Equal([InvalidInvitationMessage], exception.Errors[nameof(RegisterCommand.InvitationToken)]);
        Assert.Empty(_users.All);
        Assert.Empty(_refreshTokens.All);
        Assert.Equal(0, _users.SaveChangesCount);
    }

    [Fact]
    public async Task Refuse_une_invitation_en_attente_dont_l_echeance_est_atteinte()
    {
        var invitation = SeedInvitation();
        var handler = CreateHandler(new DateTimeOffset(invitation.ExpiresAt));

        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => handler.Handle(InvitedCommand(), CancellationToken.None));

        Assert.Equal([InvalidInvitationMessage], exception.Errors[nameof(RegisterCommand.InvitationToken)]);
        Assert.Equal(InvitationStatus.Pending, invitation.Status);
        Assert.Empty(_users.All);
    }

    [Fact]
    public async Task Refuse_un_token_d_invitation_inconnu()
    {
        SeedInvitation();

        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _handler.Handle(InvitedCommand() with { InvitationToken = "autre-token" }, CancellationToken.None));

        Assert.Equal([InvalidInvitationMessage], exception.Errors[nameof(RegisterCommand.InvitationToken)]);
        Assert.Empty(_users.All);
    }

    [Fact]
    public async Task Refuse_une_invitation_dont_l_email_a_deja_un_compte_et_la_laisse_en_attente()
    {
        var invitation = SeedInvitation();
        _users.Seed(new User { Id = 1, Email = InvitationEmail });

        var exception = await Assert.ThrowsAsync<ConflictException>(
            () => _handler.Handle(InvitedCommand(), CancellationToken.None));

        Assert.Equal("Un compte existe déjà avec cet email.", exception.Message);
        Assert.Equal(InvitationStatus.Pending, invitation.Status);
        Assert.Null(invitation.AcceptedUser);
        Assert.Empty(_refreshTokens.All);
        Assert.Equal(0, _users.SaveChangesCount);
    }

    [Fact]
    public async Task Sans_invitation_ne_touche_a_aucune_invitation()
    {
        var invitation = SeedInvitation();

        await _handler.Handle(ValidCommand(), CancellationToken.None);

        Assert.Equal("marie@mycharmly.eu", _users.All.Single().Email);
        Assert.Equal(InvitationStatus.Pending, invitation.Status);
    }
}

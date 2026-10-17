using JewelryShop.API.Tests.Fakes;
using JewelryShop.Common.Exceptions;
using JewelryShop.Features.Invitations.Entities;
using JewelryShop.Features.Invitations.Queries;
using JewelryShop.Features.Users.Entities;
using JewelryShop.Infrastructure.Services;

namespace JewelryShop.API.Tests.Invitations;

public class GetInvitationByTokenQueryHandlerTests
{
    private const string Token = "token-de-l-invitation";
    private const string InvalidMessage = "Cette invitation est invalide ou a expiré.";

    private static readonly DateTime _createdAt = new(2026, 10, 15, 8, 0, 0, DateTimeKind.Utc);
    private static readonly User _admin = new() { Id = 1, Name = "Marie", Surname = "Espinosa" };

    private readonly FakeInvitationRepository _invitations = new();
    private readonly SecureTokenGenerator _tokenGenerator = new();
    private readonly FakeTimeProvider _timeProvider = new(_createdAt.AddDays(1));
    private readonly GetInvitationByTokenQueryHandler _handler;

    public GetInvitationByTokenQueryHandlerTests()
    {
        _handler = new GetInvitationByTokenQueryHandler(_invitations, _tokenGenerator, _timeProvider);
    }

    private Invitation SeedPending()
    {
        var invitation = Invitation.Create("nouvelle@exemple.com", _tokenGenerator.Hash(Token), _admin, _createdAt);
        _invitations.Seed(invitation);
        return invitation;
    }

    [Fact]
    public async Task Renvoie_l_email_et_l_echeance_d_une_invitation_en_attente()
    {
        var invitation = SeedPending();

        var preview = await _handler.Handle(new GetInvitationByTokenQuery(Token), CancellationToken.None);

        Assert.Equal("nouvelle@exemple.com", preview.Email);
        Assert.Equal(invitation.ExpiresAt, preview.ExpiresAt);
    }

    [Fact]
    public async Task Repond_404_pour_un_token_inconnu()
    {
        SeedPending();

        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _handler.Handle(new GetInvitationByTokenQuery("autre-token"), CancellationToken.None));

        Assert.Equal(InvalidMessage, exception.Message);
    }

    [Fact]
    public async Task Repond_404_pour_une_invitation_en_attente_dont_l_echeance_est_atteinte()
    {
        var invitation = SeedPending();
        _timeProvider.Now = invitation.ExpiresAt;

        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _handler.Handle(new GetInvitationByTokenQuery(Token), CancellationToken.None));

        Assert.Equal(InvalidMessage, exception.Message);
    }

    [Theory]
    [InlineData(InvitationStatus.Accepted)]
    [InlineData(InvitationStatus.Expired)]
    [InlineData(InvitationStatus.Revoked)]
    public async Task Repond_404_pour_une_invitation_qui_n_est_plus_en_attente(InvitationStatus status)
    {
        var invitation = SeedPending();
        typeof(Invitation).GetProperty(nameof(Invitation.Status))!.SetValue(invitation, status);

        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _handler.Handle(new GetInvitationByTokenQuery(Token), CancellationToken.None));

        Assert.Equal(InvalidMessage, exception.Message);
    }
}

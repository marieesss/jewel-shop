using JewelryShop.API.Tests.Fakes;
using JewelryShop.Common.Exceptions;
using JewelryShop.Features.Invitations.Commands;
using JewelryShop.Features.Invitations.Entities;

namespace JewelryShop.API.Tests.Invitations;

public class RevokeInvitationCommandHandlerTests
{
    private const long InvitationId = 10;

    private static readonly DateTimeOffset _now = new(2026, 10, 15, 8, 0, 0, TimeSpan.Zero);

    private readonly FakeInvitationRepository _invitations = new();
    private readonly RevokeInvitationCommandHandler _handler;

    public RevokeInvitationCommandHandlerTests()
    {
        _handler = new RevokeInvitationCommandHandler(_invitations, new FakeTimeProvider(_now));
    }

    [Theory]
    [InlineData(InvitationStatus.Pending, -2)]
    [InlineData(InvitationStatus.Pending, -8)]
    [InlineData(InvitationStatus.Expired, -10)]
    public async Task Revoque_une_invitation_en_attente_depassee_ou_non_ou_expiree(InvitationStatus status, int createdDaysAgo)
    {
        var invitation = Seed(status, _now.UtcDateTime.AddDays(createdDaysAgo));

        await _handler.Handle(new RevokeInvitationCommand(InvitationId), CancellationToken.None);

        Assert.Equal(InvitationStatus.Revoked, invitation.Status);
        Assert.Equal(_now.UtcDateTime, invitation.RevokedAt);
        Assert.Single(_invitations.All);
        Assert.Equal(1, _invitations.SaveChangesCount);
    }

    [Fact]
    public async Task Refuse_une_invitation_acceptee()
    {
        var invitation = Seed(InvitationStatus.Accepted, _now.UtcDateTime.AddDays(-2));

        var exception = await Assert.ThrowsAsync<ConflictException>(
            () => _handler.Handle(new RevokeInvitationCommand(InvitationId), CancellationToken.None));

        Assert.Equal("Cette invitation a déjà été acceptée.", exception.Message);
        Assert.Equal(InvitationStatus.Accepted, invitation.Status);
        Assert.Equal(0, _invitations.SaveChangesCount);
    }

    [Fact]
    public async Task Refuse_une_invitation_deja_revoquee()
    {
        var revokedAt = _now.UtcDateTime.AddDays(-1);
        var invitation = Seed(InvitationStatus.Revoked, _now.UtcDateTime.AddDays(-2), revokedAt);

        var exception = await Assert.ThrowsAsync<ConflictException>(
            () => _handler.Handle(new RevokeInvitationCommand(InvitationId), CancellationToken.None));

        Assert.Equal("Cette invitation est déjà révoquée.", exception.Message);
        Assert.Equal(revokedAt, invitation.RevokedAt);
        Assert.Equal(0, _invitations.SaveChangesCount);
    }

    [Fact]
    public async Task Refuse_une_invitation_inexistante()
    {
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _handler.Handle(new RevokeInvitationCommand(99), CancellationToken.None));

        Assert.Equal("Invitation introuvable.", exception.Message);
        Assert.Equal(0, _invitations.SaveChangesCount);
    }

    private Invitation Seed(InvitationStatus status, DateTime createdAt, DateTime? revokedAt = null)
    {
        var invitation = InvitationTestData.Create(InvitationId, "nouvelle@exemple.com", createdAt, status, revokedAt);
        _invitations.Seed(invitation);
        return invitation;
    }
}

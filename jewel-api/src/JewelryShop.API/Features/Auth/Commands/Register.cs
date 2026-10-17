using FluentValidation.Results;
using JewelryShop.Common.Exceptions;
using JewelryShop.Common.Interfaces;
using JewelryShop.Features.Auth.DTOs;
using JewelryShop.Infrastructure.Services;
using MediatR;

namespace JewelryShop.Features.Auth.Commands;

/// <summary>
/// Crée un compte sur invitation et ouvre la session. Le compte prend l'email de l'invitation, qui passe à Accepted :
/// compte, invitation et refresh token sont enregistrés dans la même sauvegarde.
/// </summary>
public class RegisterCommandHandler : IRequestHandler<RegisterCommand, RegisterResult>
{
    private readonly IUserRepository _users;
    private readonly IInvitationRepository _invitations;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;
    private readonly IRefreshTokenService _refreshTokens;
    private readonly ISecureTokenGenerator _tokenGenerator;
    private readonly TimeProvider _timeProvider;

    public RegisterCommandHandler(
        IUserRepository users,
        IInvitationRepository invitations,
        IPasswordHasher passwordHasher,
        IJwtService jwtService,
        ISecureTokenGenerator tokenGenerator,
        TimeProvider timeProvider,
        IRefreshTokenService refreshTokens)
    {
        _users = users;
        _invitations = invitations;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
        _refreshTokens = refreshTokens;
        _timeProvider = timeProvider;
        _tokenGenerator = tokenGenerator;
    }

    public async Task<RegisterResult> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var now = _timeProvider.GetUtcNow().UtcDateTime;
        var invitation = await GetAwaitingInvitationAsync(request.InvitationToken!, now, cancellationToken);
        var email = invitation.Email;

        if (await _users.ExistsByEmailAsync(email, cancellationToken))
            throw new ConflictException("Un compte existe déjà avec cet email.");

        var user = new User
        {
            Name = request.Name.Trim(),
            Surname = request.Surname.Trim(),
            Email = email,
            PasswordHash = _passwordHasher.Hash(request.Password),
            Birthday = request.Birthday,
            Role = UserRole.User,
            CreatedAt = _timeProvider.GetUtcNow().UtcDateTime
        };

        await _users.AddAsync(user, cancellationToken);
        invitation.Accept(user, now);
        var refreshToken = _refreshTokens.Issue(user);
        await _users.SaveChangesAsync(cancellationToken);

        var token = _jwtService.GenerateToken(user);
        return new RegisterResult(new AuthResponse(token, user.Id, user.Role.ToString()), refreshToken);
    }

    private async Task<Invitation> GetAwaitingInvitationAsync(string token, DateTime now, CancellationToken cancellationToken)
    {
        var invitation = await _invitations.GetByTokenHashAsync(_tokenGenerator.Hash(token), cancellationToken);
        if (invitation is null || !invitation.IsAwaitingAcceptance(now))
            throw new ValidationException(new[]
            {
                new ValidationFailure(nameof(RegisterCommand.InvitationToken), InvitationDefaults.InvalidInvitationMessage)
            });

        return invitation;
    }
}

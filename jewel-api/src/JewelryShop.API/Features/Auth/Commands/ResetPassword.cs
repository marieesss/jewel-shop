using FluentValidation.Results;
using JewelryShop.Common.Exceptions;
using JewelryShop.Common.Interfaces;
using JewelryShop.Infrastructure.Services;
using MediatR;

namespace JewelryShop.Features.Auth.Commands;

/// <summary>
/// Remplace le mot de passe si le token est valable, le consomme et ferme toutes les sessions ouvertes.
/// </summary>
public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand>
{
    private readonly IPasswordResetTokenRepository _resetTokens;
    private readonly ISecureTokenGenerator _tokenGenerator;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IRefreshTokenService _refreshTokens;
    private readonly TimeProvider _timeProvider;

    public ResetPasswordCommandHandler(
        IPasswordResetTokenRepository resetTokens,
        ISecureTokenGenerator tokenGenerator,
        IPasswordHasher passwordHasher,
        IRefreshTokenService refreshTokens,
        TimeProvider timeProvider)
    {
        _resetTokens = resetTokens;
        _tokenGenerator = tokenGenerator;
        _passwordHasher = passwordHasher;
        _refreshTokens = refreshTokens;
        _timeProvider = timeProvider;
    }

    public async Task Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var now = _timeProvider.GetUtcNow().UtcDateTime;
        var resetToken = await _resetTokens.GetByHashWithUserAsync(_tokenGenerator.Hash(request.Token), cancellationToken);
        if (resetToken is null || !resetToken.IsUsable(now))
            throw new ValidationException(new[]
            {
                new ValidationFailure(nameof(ResetPasswordCommand.Token), PasswordResetTokenDefaults.InvalidTokenMessage)
            });

        resetToken.User.PasswordHash = _passwordHasher.Hash(request.Password);
        resetToken.MarkUsed(now);
        await _refreshTokens.RevokeAllForUserAsync(resetToken.UserId, cancellationToken);

        await _resetTokens.SaveChangesAsync(cancellationToken);
    }
}

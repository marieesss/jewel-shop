using JewelryShop.Common.Interfaces;
using JewelryShop.Infrastructure.Email;
using MediatR;
using Microsoft.Extensions.Options;

namespace JewelryShop.Features.Auth.Commands;

/// <summary>
/// Envoie un lien de réinitialisation si un compte existe. Ne dit jamais si l'email est inscrit :
/// l'appelant répond de la même façon dans les deux cas.
/// </summary>
public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand>
{
    private readonly IUserRepository _users;
    private readonly IPasswordResetTokenRepository _resetTokens;
    private readonly ISecureTokenGenerator _tokenGenerator;
    private readonly IEmailQueue _emailQueue;
    private readonly AppOptions _appOptions;
    private readonly TimeProvider _timeProvider;

    public ForgotPasswordCommandHandler(
        IUserRepository users,
        IPasswordResetTokenRepository resetTokens,
        ISecureTokenGenerator tokenGenerator,
        IEmailQueue emailQueue,
        IOptions<AppOptions> appOptions,
        TimeProvider timeProvider)
    {
        _users = users;
        _resetTokens = resetTokens;
        _tokenGenerator = tokenGenerator;
        _emailQueue = emailQueue;
        _appOptions = appOptions.Value;
        _timeProvider = timeProvider;
    }

    public async Task Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _users.GetByEmailAsync(email, cancellationToken);
        if (user is null)
            return;

        await DeletePendingTokensAsync(user.Id, cancellationToken);

        var token = _tokenGenerator.Generate();
        var now = _timeProvider.GetUtcNow().UtcDateTime;
        _resetTokens.Add(PasswordResetToken.Create(user, _tokenGenerator.Hash(token), now));
        _emailQueue.Enqueue(user.Email, EmailTemplates.PasswordReset, new
        {
            FirstName = user.Name,
            ResetUrl = _appOptions.FrontendUrl($"/reset-password?token={token}")
        });

        await _users.SaveChangesAsync(cancellationToken);
    }

    // Une nouvelle demande invalide les liens envoyés avant elle.
    private async Task DeletePendingTokensAsync(long userId, CancellationToken cancellationToken)
    {
        foreach (var pending in await _resetTokens.GetUnusedByUserIdAsync(userId, cancellationToken))
            _resetTokens.Delete(pending);
    }
}

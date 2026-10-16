using Coravel.Invocable;

namespace JewelryShop.Features.Auth.Jobs;

/// <summary>Supprime les tokens de réinitialisation expirés ou déjà utilisés : ils ne servent plus à rien.</summary>
public class PasswordResetTokenPurgeJob : IInvocable, ICancellableInvocable
{
    private readonly IPasswordResetTokenRepository _repository;

    public PasswordResetTokenPurgeJob(IPasswordResetTokenRepository repository)
    {
        _repository = repository;
    }

    public CancellationToken CancellationToken { get; set; }

    public Task Invoke() => _repository.DeleteExpiredOrUsedAsync(CancellationToken);
}

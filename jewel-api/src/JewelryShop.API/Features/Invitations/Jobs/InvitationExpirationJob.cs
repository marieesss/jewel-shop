using Coravel.Invocable;

namespace JewelryShop.Features.Invitations.Jobs;

/// <summary>
/// Passe à Expired les invitations en attente dont l'échéance est atteinte, pour que la base reflète leur statut.
/// Le code ne dépend pas de cette tâche : <see cref="Invitation.IsExpired"/> fait foi entre deux passages.
/// </summary>
public class InvitationExpirationJob : IInvocable, ICancellableInvocable
{
    private readonly IInvitationRepository _repository;

    public InvitationExpirationJob(IInvitationRepository repository)
    {
        _repository = repository;
    }

    public CancellationToken CancellationToken { get; set; }

    public Task Invoke() => _repository.ExpirePendingAsync(CancellationToken);
}

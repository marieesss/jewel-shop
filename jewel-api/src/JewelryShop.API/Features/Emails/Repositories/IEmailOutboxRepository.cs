using JewelryShop.Application.Common.Persistence;

namespace JewelryShop.Application.Features.Emails;

public interface IEmailOutboxRepository : IRepository<EmailOutboxMessage>
{
    /// <summary>Ajoute le mail au contexte sans sauvegarder : il part avec le SaveChanges de l'appelant.</summary>
    void Add(EmailOutboxMessage message);

    /// <summary>
    /// Verrouille les mails à envoyer jusqu'à la fin de la transaction en cours.
    /// Les lignes déjà verrouillées par une autre instance de l'API sont ignorées.
    /// </summary>
    Task<IReadOnlyList<EmailOutboxMessage>> LockDueBatchAsync(int batchSize, CancellationToken ct);

    /// <summary>Exécute <paramref name="work"/> dans une transaction, validée seulement s'il réussit.</summary>
    Task InTransactionAsync(Func<Task> work, CancellationToken ct);

    Task<int> DeleteSentOlderThanAsync(TimeSpan retention, CancellationToken ct);
}

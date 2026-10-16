namespace JewelryShop.Features.Invitations.Common;

/// <summary>
/// Filtres et tri de la liste des invitations, partagés par le repository EF et les tests.
/// </summary>
public static class InvitationQueries
{
    /// <summary>
    /// Filtre sur le statut effectif, comme <see cref="Invitation.EffectiveStatus"/> : une invitation Pending
    /// dont l'échéance est atteinte est Expired, même si la tâche d'expiration ne l'a pas encore traitée.
    /// </summary>
    public static IQueryable<Invitation> WhereEffectiveStatus(
        this IQueryable<Invitation> invitations, InvitationStatus status, DateTime now) => status switch
        {
            InvitationStatus.Pending => invitations.Where(i => i.Status == InvitationStatus.Pending && i.ExpiresAt > now),
            InvitationStatus.Expired => invitations.Where(i => i.Status == InvitationStatus.Expired
                || (i.Status == InvitationStatus.Pending && i.ExpiresAt <= now)),
            _ => invitations.Where(i => i.Status == status)
        };

    /// <summary>De la plus récente à la plus ancienne, l'id départageant deux créations simultanées.</summary>
    public static IQueryable<Invitation> OrderByNewest(this IQueryable<Invitation> invitations)
        => invitations.OrderByDescending(i => i.CreatedAt).ThenByDescending(i => i.Id);

    /// <summary>
    /// Lit le statut demandé par la liste : Pending par défaut, nom du statut sans tenir compte de la casse.
    /// Une valeur numérique est refusée.
    /// </summary>
    public static bool TryParseStatus(string? value, out InvitationStatus status)
    {
        status = InvitationStatus.Pending;
        if (string.IsNullOrWhiteSpace(value))
            return true;

        var name = Enum.GetNames<InvitationStatus>()
            .FirstOrDefault(n => string.Equals(n, value.Trim(), StringComparison.OrdinalIgnoreCase));
        if (name is null)
            return false;

        status = Enum.Parse<InvitationStatus>(name);
        return true;
    }
}

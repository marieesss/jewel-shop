namespace JewelryShop.Application.Common.Interfaces;

/// <summary>
/// Tâche exécutée en arrière-plan par PeriodicJobsService, dans un scope DI dédié.
/// À enregistrer avec services.AddScoped&lt;IPeriodicJob, ...&gt;().
/// </summary>
public interface IPeriodicJob
{
    TimeSpan Interval { get; }

    Task RunAsync(CancellationToken ct);
}

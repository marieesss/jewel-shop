using JewelryShop.Application.Common.Interfaces;

namespace JewelryShop.Infrastructure.BackgroundJobs;

/// <summary>
/// Planificateur des tâches périodiques : toutes les 10 secondes, exécute chaque tâche
/// dont l'intervalle est écoulé. L'échec d'une tâche est journalisé sans arrêter les autres.
/// </summary>
public class PeriodicJobsService : BackgroundService
{
    private static readonly TimeSpan _tickInterval = TimeSpan.FromSeconds(10);

    // Le timer peut se déclencher un peu en avance : sans cette marge, une tâche de 10 s
    // manquerait un tour sur deux.
    private static readonly TimeSpan _dueTolerance = TimeSpan.FromSeconds(1);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<PeriodicJobsService> _logger;
    private readonly Dictionary<Type, DateTimeOffset> _lastRuns = new();

    public PeriodicJobsService(
        IServiceScopeFactory scopeFactory, TimeProvider timeProvider, ILogger<PeriodicJobsService> logger)
    {
        _scopeFactory = scopeFactory;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(_tickInterval, _timeProvider);
        do
        {
            await RunDueJobsAsync(stoppingToken);
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    internal async Task RunDueJobsAsync(CancellationToken ct)
    {
        var now = _timeProvider.GetUtcNow();
        foreach (var jobType in GetDueJobTypes(now))
        {
            _lastRuns[jobType] = now;
            await RunJobAsync(jobType, ct);
        }
    }

    private List<Type> GetDueJobTypes(DateTimeOffset now)
    {
        using var scope = _scopeFactory.CreateScope();
        return scope.ServiceProvider.GetServices<IPeriodicJob>()
            .Where(job => IsDue(job, now))
            .Select(job => job.GetType())
            .ToList();
    }

    private bool IsDue(IPeriodicJob job, DateTimeOffset now)
        => !_lastRuns.TryGetValue(job.GetType(), out var lastRun)
            || now - lastRun >= job.Interval - _dueTolerance;

    private async Task RunJobAsync(Type jobType, CancellationToken ct)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var job = scope.ServiceProvider.GetServices<IPeriodicJob>().First(j => j.GetType() == jobType);
        try
        {
            await job.RunAsync(ct);
        }
        catch (Exception ex) when (!ct.IsCancellationRequested)
        {
            _logger.LogError(ex, "La tâche périodique {Job} a échoué.", jobType.Name);
        }
    }
}

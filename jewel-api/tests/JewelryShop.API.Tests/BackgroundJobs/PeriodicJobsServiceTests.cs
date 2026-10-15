using JewelryShop.API.Tests.Fakes;
using JewelryShop.Application.Common.Interfaces;
using JewelryShop.Infrastructure.BackgroundJobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace JewelryShop.API.Tests.BackgroundJobs;

public class PeriodicJobsServiceTests
{
    private readonly FakeTimeProvider _timeProvider = new(new DateTimeOffset(2026, 10, 15, 8, 0, 0, TimeSpan.Zero));
    private readonly ListLogger<PeriodicJobsService> _logger = new();
    private readonly JobRuns _runs = new();

    private PeriodicJobsService BuildService(Action<IServiceCollection> registerJobs)
    {
        var services = new ServiceCollection();
        services.AddSingleton(_runs);
        registerJobs(services);
        return new PeriodicJobsService(
            services.BuildServiceProvider().GetRequiredService<IServiceScopeFactory>(), _timeProvider, _logger);
    }

    [Fact]
    public async Task Une_tache_en_echec_n_empeche_pas_les_autres_de_s_executer()
    {
        var service = BuildService(services =>
        {
            services.AddScoped<IPeriodicJob, FailingJob>();
            services.AddScoped<IPeriodicJob, TenSecondsJob>();
        });

        await service.RunDueJobsAsync(CancellationToken.None);

        Assert.Equal(1, _runs.Count(nameof(FailingJob)));
        Assert.Equal(1, _runs.Count(nameof(TenSecondsJob)));
    }

    [Fact]
    public async Task L_echec_d_une_tache_est_journalise_en_erreur()
    {
        var service = BuildService(services => services.AddScoped<IPeriodicJob, FailingJob>());

        await service.RunDueJobsAsync(CancellationToken.None);

        var entry = Assert.Single(_logger.Entries);
        Assert.Equal(LogLevel.Error, entry.Level);
        Assert.Contains(nameof(FailingJob), entry.Message);
    }

    [Fact]
    public async Task La_boucle_continue_apres_l_echec_d_une_tache()
    {
        var service = BuildService(services => services.AddScoped<IPeriodicJob, FailingJob>());

        await service.RunDueJobsAsync(CancellationToken.None);
        _timeProvider.Now += TimeSpan.FromSeconds(10);
        await service.RunDueJobsAsync(CancellationToken.None);

        Assert.Equal(2, _runs.Count(nameof(FailingJob)));
    }

    [Fact]
    public async Task Une_tache_n_est_pas_relancee_avant_la_fin_de_son_intervalle()
    {
        var service = BuildService(services => services.AddScoped<IPeriodicJob, OneHourJob>());

        await service.RunDueJobsAsync(CancellationToken.None);
        _timeProvider.Now += TimeSpan.FromMinutes(30);
        await service.RunDueJobsAsync(CancellationToken.None);

        Assert.Equal(1, _runs.Count(nameof(OneHourJob)));
    }

    [Fact]
    public async Task Une_tache_est_relancee_une_fois_son_intervalle_ecoule()
    {
        var service = BuildService(services => services.AddScoped<IPeriodicJob, OneHourJob>());

        await service.RunDueJobsAsync(CancellationToken.None);
        _timeProvider.Now += TimeSpan.FromHours(1);
        await service.RunDueJobsAsync(CancellationToken.None);

        Assert.Equal(2, _runs.Count(nameof(OneHourJob)));
    }

    [Fact]
    public async Task Un_tour_legerement_en_avance_execute_quand_meme_la_tache()
    {
        var service = BuildService(services => services.AddScoped<IPeriodicJob, TenSecondsJob>());

        await service.RunDueJobsAsync(CancellationToken.None);
        _timeProvider.Now += TimeSpan.FromSeconds(9.99);
        await service.RunDueJobsAsync(CancellationToken.None);

        Assert.Equal(2, _runs.Count(nameof(TenSecondsJob)));
    }

    private class JobRuns
    {
        private readonly List<string> _names = new();

        public void Record(string name) => _names.Add(name);

        public int Count(string name) => _names.Count(n => n == name);
    }

    private class TenSecondsJob(JobRuns runs) : IPeriodicJob
    {
        public TimeSpan Interval => TimeSpan.FromSeconds(10);

        public Task RunAsync(CancellationToken ct)
        {
            runs.Record(nameof(TenSecondsJob));
            return Task.CompletedTask;
        }
    }

    private class OneHourJob(JobRuns runs) : IPeriodicJob
    {
        public TimeSpan Interval => TimeSpan.FromHours(1);

        public Task RunAsync(CancellationToken ct)
        {
            runs.Record(nameof(OneHourJob));
            return Task.CompletedTask;
        }
    }

    private class FailingJob(JobRuns runs) : IPeriodicJob
    {
        public TimeSpan Interval => TimeSpan.FromSeconds(10);

        public Task RunAsync(CancellationToken ct)
        {
            runs.Record(nameof(FailingJob));
            throw new InvalidOperationException("Base injoignable");
        }
    }
}

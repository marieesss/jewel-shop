using EvolveDb;
using EvolveDb.Migration;
using Npgsql;
using Testcontainers.PostgreSql;

namespace JewelryShop.API.IntegrationTests.Chains;

/// <summary>
/// Reprise des chaînes existantes par V7 : une base dédiée est migrée jusqu'à V6, reçoit des chaînes
/// à l'ancien format, puis passe à la dernière version, comme la base de production au déploiement.
/// </summary>
public class ChainMigrationTests : IClassFixture<ChainMigrationTests.MigratedDatabase>
{
    private readonly MigratedDatabase _database;

    public ChainMigrationTests(MigratedDatabase database) => _database = database;

    [Fact]
    public async Task Calcule_les_prix_au_metre_des_chaines_existantes()
    {
        Assert.Equal((4.00m, 10.00m), await MeterPricesAsync(1));
        Assert.Equal((21.98m, 43.96m), await MeterPricesAsync(4));
    }

    [Fact]
    public async Task Arrondit_au_pair_comme_l_api()
    {
        // 0,27 € et 0,25 € pour 200 cm donnent exactement 0,135 et 0,125 € au mètre.
        Assert.Equal((0.14m, 0.12m), await MeterPricesAsync(3));
    }

    [Fact]
    public async Task Laisse_les_prix_au_metre_vides_pour_une_longueur_nulle()
    {
        Assert.Equal((null, null), await MeterPricesAsync(2));
    }

    [Fact]
    public async Task Garde_les_anciennes_colonnes_et_met_le_fermoir_a_zero()
    {
        await using var connection = await OpenAsync();
        await using var command = new NpgsqlCommand(
            "SELECT cost, price, length, clasp_cost, clasp_price FROM chains WHERE id = 1", connection);
        await using var reader = await command.ExecuteReaderAsync();
        Assert.True(await reader.ReadAsync());

        Assert.Equal(20m, reader.GetDecimal(0));
        Assert.Equal(50m, reader.GetDecimal(1));
        Assert.Equal(500m, reader.GetDecimal(2));
        Assert.Equal(0m, reader.GetDecimal(3));
        Assert.Equal(0m, reader.GetDecimal(4));
    }

    [Fact]
    public async Task Cree_un_rouleau_entier_par_chaine_de_longueur_non_nulle()
    {
        await using var connection = await OpenAsync();
        await using var command = new NpgsqlCommand(
            "SELECT chain_id, initial_length_cm, remaining_length_cm FROM chain_rolls ORDER BY chain_id", connection);
        await using var reader = await command.ExecuteReaderAsync();
        var rolls = new List<(long, int, int)>();
        while (await reader.ReadAsync())
            rolls.Add((reader.GetInt64(0), reader.GetInt32(1), reader.GetInt32(2)));

        Assert.Equal([(1L, 500, 500), (3L, 200, 200), (4L, 46, 46)], rolls);
    }

    private Task<NpgsqlConnection> OpenAsync() => _database.OpenAsync();

    private async Task<(decimal?, decimal?)> MeterPricesAsync(long chainId)
    {
        await using var connection = await OpenAsync();
        await using var command = new NpgsqlCommand(
            "SELECT cost_per_meter, price_per_meter FROM chains WHERE id = @id", connection);
        command.Parameters.AddWithValue("id", chainId);
        await using var reader = await command.ExecuteReaderAsync();
        Assert.True(await reader.ReadAsync());
        return (
            reader.IsDBNull(0) ? null : reader.GetDecimal(0),
            reader.IsDBNull(1) ? null : reader.GetDecimal(1));
    }

    public class MigratedDatabase : IAsyncLifetime
    {
        private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:16-alpine").Build();

        public async Task InitializeAsync()
        {
            await _container.StartAsync();
            Migrate(new MigrationVersion("6"));
            await ExecuteAsync("""
                INSERT INTO chains (id, name, color, cost, price, length) VALUES
                    (1, 'Gourmette', 'Gold', 20, 50, 500),
                    (2, 'Longueur nulle', 'Gold', 5, 10, 0),
                    (3, 'Au milieu de deux centimes', 'Silver', 0.27, 0.25, 200),
                    (4, 'Longueur décimale', 'Silver', 10, 20, 45.5);
                """);
            Migrate(targetVersion: null);
        }

        public Task DisposeAsync() => _container.DisposeAsync().AsTask();

        public async Task<NpgsqlConnection> OpenAsync()
        {
            var connection = new NpgsqlConnection(_container.GetConnectionString());
            await connection.OpenAsync();
            return connection;
        }

        private void Migrate(MigrationVersion? targetVersion)
        {
            using var connection = new NpgsqlConnection(_container.GetConnectionString());
            var evolve = new Evolve(connection)
            {
                Locations = [Path.Combine(AppContext.BaseDirectory, "db", "migrations")],
                IsEraseDisabled = true
            };
            if (targetVersion is not null)
                evolve.TargetVersion = targetVersion;
            evolve.Migrate();
        }

        private async Task ExecuteAsync(string sql)
        {
            await using var connection = await OpenAsync();
            await using var command = new NpgsqlCommand(sql, connection);
            await command.ExecuteNonQueryAsync();
        }
    }
}

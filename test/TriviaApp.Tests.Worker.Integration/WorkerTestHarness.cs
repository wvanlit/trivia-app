using Dapper;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Networks;
using Npgsql;
using Testcontainers.PostgreSql;
using TriviaApp.Domain.Model;
using TriviaApp.Infrastructure;

namespace TriviaApp.Tests.Worker.Integration;

internal sealed class TestHarness : IAsyncDisposable
{
    private readonly INetwork _network;
    private readonly PostgreSqlContainer _postgres;
    private readonly NpgsqlDataSource _dataSource;

    internal TriviaRepository Repository { get; }

    private TestHarness(
        INetwork network,
        PostgreSqlContainer postgres,
        NpgsqlDataSource dataSource,
        TriviaRepository repository)
    {
        _network = network;
        _postgres = postgres;
        _dataSource = dataSource;
        Repository = repository;
    }

    public static async Task<TestHarness> Create()
    {
        var network = new NetworkBuilder().Build();
        await network.CreateAsync();

        const string username = "postgres";
        const string password = "postgres";

        var postgres = new PostgreSqlBuilder()
            .WithDatabase("trivia")
            .WithUsername(username)
            .WithPassword(password)
            .WithNetwork(network)
            .WithNetworkAliases("postgres")
            .Build();

        await postgres.StartAsync();

        await RunFlyway(network, username, password);

        var dataSource = new NpgsqlDataSourceBuilder(postgres.GetConnectionString()).Build();
        var repository = new TriviaRepository(dataSource);

        return new TestHarness(network, postgres, dataSource, repository);
    }

    public async Task<int> CountCategories(CancellationToken cancellationToken)
    {
        const string sql = "SELECT COUNT(*) FROM category;";
        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        return await connection.ExecuteScalarAsync<int>(new CommandDefinition(sql, cancellationToken: cancellationToken));
    }

    public async Task<int> CountQuestions(CancellationToken cancellationToken)
    {
        const string sql = "SELECT COUNT(*) FROM questions;";
        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        return await connection.ExecuteScalarAsync<int>(new CommandDefinition(sql, cancellationToken: cancellationToken));
    }

    public async Task<int> CountQuestionsInCategory(CategoryId categoryId, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT COUNT(*)
            FROM questions
            WHERE category_id = @CategoryId;
            """;

        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        return await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(
                sql,
                new { CategoryId = (long)categoryId },
                cancellationToken: cancellationToken));
    }

    public async ValueTask DisposeAsync()
    {
        await _dataSource.DisposeAsync();
        await _postgres.DisposeAsync();
        await _network.DeleteAsync();
    }

    private static async Task RunFlyway(INetwork network, string username, string password)
    {
        var root = RepositoryRootLocator.GetRootPath();
        var migrationPath = Path.Combine(root, "infrastructure", "postgres");

        var flyway = new ContainerBuilder()
            .WithImage("docker.io/flyway/flyway:11-alpine")
            .WithNetwork(network)
            .WithBindMount(migrationPath, "/flyway/conf")
            .WithEnvironment("FLYWAY_URL", "jdbc:postgresql://postgres:5432/trivia")
            .WithEnvironment("FLYWAY_USER", username)
            .WithEnvironment("FLYWAY_PASSWORD", password)
            .WithCommand("migrate")
            .Build();

        await flyway.StartAsync();

        var exitCode = await flyway.GetExitCodeAsync();

        if (exitCode != 0)
        {
            throw new InvalidOperationException($"Flyway migration failed with exit code {exitCode}.");
        }

        await flyway.DisposeAsync();
    }

    private static class RepositoryRootLocator
    {
        public static string GetRootPath()
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);

            while (directory != null && !File.Exists(Path.Combine(directory.FullName, "trivia-app.slnx")))
            {
                directory = directory.Parent;
            }

            return directory?.FullName ?? throw new DirectoryNotFoundException("Repository root not found for integration tests.");
        }
    }
}

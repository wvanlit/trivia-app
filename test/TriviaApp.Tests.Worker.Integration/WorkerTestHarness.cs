using Dapper;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Networks;
using Npgsql;
using Testcontainers.PostgreSql;
using TriviaApp.Domain.Model;
using TriviaApp.Infrastructure;
using TriviaApp.Tests.Shared;

namespace TriviaApp.Tests.Worker.Integration;

public sealed class TestHarness : IAsyncDisposable
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

        await FlywayMigration.Run(network, username, password);

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

}

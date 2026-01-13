using System.Text.Json;
using Dapper;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Networks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Testcontainers.PostgreSql;
using TriviaApp.Tests.Shared;

namespace TriviaApp.Tests.Api.Integration;

public sealed class ApiTestHarness : IAsyncDisposable
{
    private const string TriviaConnectionKey = "ConnectionStrings__trivia";
    private readonly INetwork _network;
    private readonly PostgreSqlContainer _postgres;
    private readonly NpgsqlDataSource _dataSource;
    private readonly ApiFactory _factory;
    private readonly string? _previousConnectionString;

    public HttpClient Client { get; }

    private ApiTestHarness(
        INetwork network,
        PostgreSqlContainer postgres,
        NpgsqlDataSource dataSource,
        ApiFactory factory,
        HttpClient client,
        string? previousConnectionString)
    {
        _network = network;
        _postgres = postgres;
        _dataSource = dataSource;
        _factory = factory;
        _previousConnectionString = previousConnectionString;

        Client = client;
    }

    public static async Task<ApiTestHarness> Create()
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

        var connectionString = postgres.GetConnectionString();
        var dataSource = new NpgsqlDataSourceBuilder(connectionString).Build();
        var previousConnectionString = Environment.GetEnvironmentVariable(TriviaConnectionKey);
        Environment.SetEnvironmentVariable(TriviaConnectionKey, connectionString);
        var factory = new ApiFactory(connectionString);
        var client = factory.CreateClient();

        return new ApiTestHarness(network, postgres, dataSource, factory, client, previousConnectionString);
    }

    public async Task InsertCategory(long categoryId, string name)
    {
        const string sql = "INSERT INTO category (category_id, name) VALUES (@CategoryId, @Name);";

        await using var connection = await _dataSource.OpenConnectionAsync();

        await connection.ExecuteAsync(new CommandDefinition(sql, new { CategoryId = categoryId, Name = name }));
    }

    public async Task<long> InsertQuestion(
        long categoryId,
        string question,
        string correctAnswer,
        IReadOnlyList<string> options,
        string difficulty)
    {
        const string sql = """
            INSERT INTO questions (category_id, question, correct_answer, options, difficulty)
            VALUES (@CategoryId, @Question, @CorrectAnswer, CAST(@Options AS jsonb), @Difficulty)
            RETURNING question_id;
            """;

        await using var connection = await _dataSource.OpenConnectionAsync();

        return await connection.ExecuteScalarAsync<long>(
            new CommandDefinition(
                sql,
                new
                {
                    CategoryId = categoryId,
                    Question = question,
                    CorrectAnswer = correctAnswer,
                    Options = JsonSerializer.Serialize(options),
                    Difficulty = difficulty
                }));
    }

    // Clears tables between tests so the shared container stays deterministic.
    public async Task ResetDatabase()
    {
        const string sql = "TRUNCATE TABLE questions, category RESTART IDENTITY CASCADE;";

        await using var connection = await _dataSource.OpenConnectionAsync();

        await connection.ExecuteAsync(new CommandDefinition(sql));
    }

    public async ValueTask DisposeAsync()
    {
        Client.Dispose();
        _factory.Dispose();

        await _dataSource.DisposeAsync();
        await _postgres.DisposeAsync();
        await _network.DeleteAsync();

        Environment.SetEnvironmentVariable(TriviaConnectionKey, _previousConnectionString);
    }

    private sealed class ApiFactory(string connectionString) : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration(
                (context, config) => config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:trivia"] = connectionString
                }));
        }
    }

}

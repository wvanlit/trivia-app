using System.Text.Json;
using Aspire.Hosting;
using Aspire.Hosting.Testing;
using Dapper;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Networks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
using Testcontainers.PostgreSql;
using TriviaApp.Tests.Shared;

namespace TriviaApp.Tests.EndToEnd.Setup;

internal sealed class TestHarness : IAsyncDisposable
{
    private const string TriviaConnectionKey = "ConnectionStrings__trivia";
    private const string DisableWorkerKey = "TRIVIA_DISABLE_WORKER";
    private static readonly TimeSpan StartupTimeout = TimeSpan.FromMinutes(2);

    private readonly INetwork _network;
    private readonly PostgreSqlContainer _postgres;
    private readonly NpgsqlDataSource _dataSource;
    private readonly DistributedApplication _app;
    private readonly string? _previousConnectionString;
    private readonly string? _previousDisableWorker;

    internal Uri FrontendBaseUrl { get; }

    private TestHarness(
        INetwork network,
        PostgreSqlContainer postgres,
        NpgsqlDataSource dataSource,
        DistributedApplication app,
        Uri frontendBaseUrl,
        string? previousConnectionString,
        string? previousDisableWorker)
    {
        _network = network;
        _postgres = postgres;
        _dataSource = dataSource;
        _app = app;
        _previousConnectionString = previousConnectionString;
        _previousDisableWorker = previousDisableWorker;
        FrontendBaseUrl = frontendBaseUrl;
    }

    internal static async Task<TestHarness> Create(CancellationToken cancellationToken = default)
    {
        var network = new NetworkBuilder().Build();
        await network.CreateAsync(cancellationToken);

        const string username = "postgres";
        const string password = "postgres";

        var postgres = new PostgreSqlBuilder()
            .WithDatabase("trivia")
            .WithUsername(username)
            .WithPassword(password)
            .WithNetwork(network)
            .WithNetworkAliases("postgres")
            .Build();

        await postgres.StartAsync(cancellationToken);

        await FlywayMigration.Run(network, username, password);

        var connectionString = postgres.GetConnectionString();
        var dataSource = new NpgsqlDataSourceBuilder(connectionString).Build();

        await SeedData(dataSource, cancellationToken);

        var previousConnectionString = Environment.GetEnvironmentVariable(TriviaConnectionKey);
        var previousDisableWorker = Environment.GetEnvironmentVariable(DisableWorkerKey);

        Environment.SetEnvironmentVariable(TriviaConnectionKey, connectionString);
        Environment.SetEnvironmentVariable(DisableWorkerKey, "true");

        var builder = await DistributedApplicationTestingBuilder.CreateAsync<Projects.TriviaApp_AppHost>(cancellationToken);

        builder.Services.ConfigureHttpClientDefaults(http => http.AddStandardResilienceHandler());

        builder.Services.AddLogging(logging =>
        {
            logging.AddFilter("Microsoft", LogLevel.Warning);
            logging.AddFilter("System", LogLevel.Warning);
            logging.AddFilter("Aspire.Hosting", LogLevel.Information);
        });

        var app = await builder.BuildAsync(cancellationToken);

        await app.StartAsync(cancellationToken).WaitAsync(StartupTimeout, cancellationToken);

        using var healthCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        healthCts.CancelAfter(StartupTimeout);

        await app.ResourceNotifications.WaitForResourceHealthyAsync("frontend", healthCts.Token);

        var frontendBaseUrl = app.GetEndpoint("frontend");

        return new TestHarness(
            network,
            postgres,
            dataSource,
            app,
            frontendBaseUrl,
            previousConnectionString,
            previousDisableWorker);
    }

    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        try
        {
            await _app.DisposeAsync();
            await _dataSource.DisposeAsync();
            await _postgres.DisposeAsync();
            await _network.DeleteAsync();
        }
        finally
        {
            Environment.SetEnvironmentVariable(TriviaConnectionKey, _previousConnectionString);
            Environment.SetEnvironmentVariable(DisableWorkerKey, _previousDisableWorker);
        }
    }

    private static async Task SeedData(NpgsqlDataSource dataSource, CancellationToken cancellationToken)
    {
        const string insertCategorySql = "INSERT INTO category (category_id, name) VALUES (@CategoryId, @Name);";
        const string insertQuestionSql = """
            INSERT INTO questions (category_id, question, correct_answer, options, difficulty)
            VALUES (@CategoryId, @Question, @CorrectAnswer, CAST(@Options AS jsonb), @Difficulty);
            """;

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);

        await connection.ExecuteAsync(new CommandDefinition(
            insertCategorySql,
            new { TestData.CategoryId, Name = TestData.CategoryName },
            cancellationToken: cancellationToken));

        foreach (var question in TestData.Questions)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var correctAnswer = question.Options[question.CorrectIndex];

            await connection.ExecuteAsync(new CommandDefinition(
                insertQuestionSql,
                new
                {
                    TestData.CategoryId,
                    Question = question.Text,
                    CorrectAnswer = correctAnswer,
                    Options = JsonSerializer.Serialize(question.Options),
                    TestData.Difficulty
                },
                cancellationToken: cancellationToken));
        }
    }
}

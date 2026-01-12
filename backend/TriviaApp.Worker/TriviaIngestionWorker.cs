using System.Diagnostics.CodeAnalysis;
using TriviaApp.Domain.Workflows;

namespace TriviaApp.Worker;

[SuppressMessage(
    "Performance",
    "CA1812:Avoid uninstantiated internal classes",
    Justification = "Instantiated as Hosted Service.")]
internal sealed class TriviaIngestionWorker(
    IServiceScopeFactory scopeFactory,
    ILogger<TriviaIngestionWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();

            var workflow = scope.ServiceProvider.GetRequiredService<TriviaIngestionWorkflow>();

            await workflow.Run(stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("Trivia ingestion canceled.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Trivia ingestion failed.");
            throw;
        }
    }

}

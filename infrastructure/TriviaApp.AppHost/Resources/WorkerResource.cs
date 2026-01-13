using Microsoft.Extensions.Configuration;

namespace TriviaApp.AppHost.Resources;

internal static class DistributedApplicationBuilderWorkerExtensions
{
    internal static void AddWorker(
        this IDistributedApplicationBuilder builder,
        TriviaDatabaseResources triviaResources)
    {
        var disableWorker = builder.Configuration.GetValue<bool>("TRIVIA_DISABLE_WORKER");

        if (disableWorker)
            return;

        var worker = builder
            .AddProject<Projects.TriviaApp_Worker>("worker")
            .WithReference(triviaResources.Database);

        if (triviaResources.Flyway is not null)
            worker.WaitFor(triviaResources.Flyway);
    }
}

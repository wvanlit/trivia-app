namespace TriviaApp.AppHost.Resources;

internal static class DistributedApplicationBuilderApiExtensions
{
    internal static IResourceBuilder<ProjectResource> AddTriviaApi(
        this IDistributedApplicationBuilder builder,
        TriviaDatabaseResources triviaResources)
    {
        var api = builder.AddProject<Projects.TriviaApp_API>("api")
            .WithHttpHealthCheck("/health")
            .WithExternalHttpEndpoints()
            .WithReference(triviaResources.Database);

        if (triviaResources.Flyway is not null)
        {
            api = api.WaitFor(triviaResources.Flyway);
        }

        return api;
    }
}

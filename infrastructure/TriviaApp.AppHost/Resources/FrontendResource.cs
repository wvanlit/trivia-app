namespace TriviaApp.AppHost.Resources;

internal static class DistributedApplicationBuilderFrontendExtensions
{
    internal static void AddFrontend(
        this IDistributedApplicationBuilder builder,
        IResourceBuilder<ProjectResource> api)
    {
        var frontend = builder
            .AddViteApp("frontend", "../../frontend")
            .WithReference(api)
            .WaitFor(api);

        api.PublishWithContainerFiles(frontend, "wwwroot");
    }
}

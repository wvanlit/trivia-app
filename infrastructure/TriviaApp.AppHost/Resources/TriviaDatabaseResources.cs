namespace TriviaApp.AppHost.Resources;

internal sealed record TriviaDatabaseResources(
    IResourceBuilder<IResourceWithConnectionString> Database,
    IResourceBuilder<ContainerResource>? Flyway);

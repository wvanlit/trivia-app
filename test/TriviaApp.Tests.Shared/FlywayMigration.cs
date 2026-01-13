using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Networks;

namespace TriviaApp.Tests.Shared;

public static class FlywayMigration
{
    public static async Task Run(INetwork network, string username, string password)
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

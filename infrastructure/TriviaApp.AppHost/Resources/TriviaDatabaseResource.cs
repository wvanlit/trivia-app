using Microsoft.Extensions.Configuration;

namespace TriviaApp.AppHost.Resources;

internal static class DistributedApplicationBuilderDatabaseExtensions
{
    internal static TriviaDatabaseResources AddTriviaDatabase(this IDistributedApplicationBuilder builder)
    {
        // External connection is used for E2E tests.
        var externalTriviaConnection = builder.Configuration.GetConnectionString("trivia");
        if (!string.IsNullOrWhiteSpace(externalTriviaConnection))
        {
            var externalTriviaDb = builder.AddConnectionString("trivia");
            return new TriviaDatabaseResources(externalTriviaDb, null);
        }

        var postgres = builder.AddPostgres("postgres");
        var triviaDb = postgres.AddDatabase("trivia");

        var flyway = builder
            .AddContainer("flyway", "docker.io/flyway/flyway:11-alpine")
            .WithBindMount("../postgres", "/flyway/conf")
            .WithEnvironment("FLYWAY_URL", "jdbc:postgresql://postgres:5432/trivia")
            .WithEnvironment("FLYWAY_USER", postgres.Resource.UserNameReference)
            .WithEnvironment(
                "FLYWAY_PASSWORD",
                postgres.Resource.PasswordParameter ?? throw new InvalidOperationException("Postgres password parameter was not configured."))
            .WithArgs("migrate")
            .WaitFor(triviaDb);

        return new TriviaDatabaseResources(triviaDb, flyway);
    }
}

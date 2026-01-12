var builder = DistributedApplication.CreateBuilder(args);

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

var api = builder.AddProject<Projects.TriviaApp_API>("api")
    .WithHttpHealthCheck("/health")
    .WithExternalHttpEndpoints()
    .WithReference(triviaDb)
    .WaitFor(flyway);

var worker = builder.AddProject<Projects.TriviaApp_Worker>("worker")
    .WithReference(triviaDb)
    .WaitFor(flyway);

var frontend = builder.AddViteApp("frontend", "../../frontend")
    .WithReference(api)
    .WaitFor(api);

api.PublishWithContainerFiles(frontend, "wwwroot");

builder.Build().Run();

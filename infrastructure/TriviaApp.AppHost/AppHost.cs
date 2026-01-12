var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres");
var triviaDb = postgres.AddDatabase("trivia");

var api = builder.AddProject<Projects.TriviaApp_API>("api")
    .WithHttpHealthCheck("/health")
    .WithExternalHttpEndpoints()
    .WithReference(triviaDb)
    .WaitFor(triviaDb);

var worker = builder.AddProject<Projects.TriviaApp_Worker>("worker")
    .WithReference(triviaDb)
    .WaitFor(triviaDb);

var webfrontend = builder.AddViteApp("webfrontend", "../../frontend")
    .WithReference(api)
    .WaitFor(api);

api.PublishWithContainerFiles(webfrontend, "wwwroot");

builder.Build().Run();

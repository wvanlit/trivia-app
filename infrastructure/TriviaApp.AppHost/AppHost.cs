using TriviaApp.AppHost.Resources;

var builder = DistributedApplication.CreateBuilder(args);


var database = builder.AddTriviaDatabase();

var api = builder.AddTriviaApi(database);

builder.AddWorker(database);
builder.AddFrontend(api);

builder.Build().Run();

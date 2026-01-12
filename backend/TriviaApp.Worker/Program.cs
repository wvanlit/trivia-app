using TriviaApp.Infrastructure;
using TriviaApp.ServiceDefaults;
using TriviaApp.Worker;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddTriviaInfrastructure(builder.Configuration);
builder.Services.AddHostedService<TriviaIngestionWorker>();

builder.Build().Run();

using TriviaApp.API.Endpoints;
using TriviaApp.Domain.Extensions;
using TriviaApp.Infrastructure;
using TriviaApp.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddTriviaDataAccess(builder.Configuration);
builder.Services.AddTriviaQueries();

var app = builder.Build();

app.UseExceptionHandler();
app.MapDefaultEndpoints();
app.MapOpenApi();

var api = app.MapGroup("/api");

api.MapRandomQuestionsEndpoints();
api.MapCategoryEndpoints();
api.MapAnswerVerificationEndpoints();

app.Run();

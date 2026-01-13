using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Npgsql;
using Refit;
using TriviaApp.Domain.Extensions;
using TriviaApp.Domain.Ports;
using TriviaApp.Infrastructure.OpenTrivia;

namespace TriviaApp.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTriviaInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddTriviaDataAccess(configuration);
        services.AddOptions<OpenTriviaOptions>()
            .BindConfiguration(OpenTriviaOptions.SectionName)
            .Validate(options => options.BaseUrl is { IsAbsoluteUri: true, Scheme: "https" }, "OpenTrivia:BaseUrl must be an absolute https URL.")
            .ValidateOnStart();

        services.AddRefitClient<ITriviaApi>(
            new RefitSettings(
                new SystemTextJsonContentSerializer(
                    new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
                        PropertyNameCaseInsensitive = true
                    })))
            .ConfigureHttpClient((serviceProvider, client) =>
            {
                var options = serviceProvider.GetRequiredService<IOptions<OpenTriviaOptions>>().Value;

                client.BaseAddress = options.BaseUrl;
            });

        services.AddScoped<ITriviaSource, OpenTriviaSource>();
        services.AddTriviaIngestion();

        return services;
    }

    public static IServiceCollection AddTriviaDataAccess(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("trivia");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Connection string 'trivia' is required for trivia data access.");
        }

        var dataSource = new NpgsqlDataSourceBuilder(connectionString).Build();

        services.AddSingleton(dataSource);
        services.AddScoped<ITriviaRepository, TriviaRepository>();

        return services;
    }
}

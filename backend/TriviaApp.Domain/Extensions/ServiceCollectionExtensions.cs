using Microsoft.Extensions.DependencyInjection;
using TriviaApp.Domain.Configuration;
using TriviaApp.Domain.Workflows;

namespace TriviaApp.Domain.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers domain services for trivia ingestion workflows.
    /// </summary>
    public static IServiceCollection AddTriviaDomain(this IServiceCollection services)
    {
        services.AddOptions<TriviaIngestionOptions>()
            .BindConfiguration(TriviaIngestionOptions.SectionName)
            .Validate(options => options.MaxQuestionsPerCategory > 0, "OpenTrivia:MaxQuestionsPerCategory must be greater than 0.")
            .ValidateOnStart();

        services.AddScoped<TriviaIngestionWorkflow>();
        return services;
    }
}

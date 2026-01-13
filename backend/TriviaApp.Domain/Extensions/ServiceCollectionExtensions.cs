using Microsoft.Extensions.DependencyInjection;
using TriviaApp.Domain.Configuration;
using TriviaApp.Domain.Queries;
using TriviaApp.Domain.Workflows;

namespace TriviaApp.Domain.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers domain services for trivia ingestion workflows.
    /// </summary>
    public static IServiceCollection AddTriviaIngestion(this IServiceCollection services)
    {
        services.AddOptions<TriviaIngestionOptions>()
            .BindConfiguration(TriviaIngestionOptions.SectionName)
            .Validate(options => options.MaxQuestionsPerCategory > 0, "OpenTrivia:MaxQuestionsPerCategory must be greater than 0.")
            .ValidateOnStart();

        services.AddScoped<TriviaIngestionWorkflow>();

        return services;
    }

    /// <summary>
    /// Registers domain services for querying trivia.
    /// </summary>
    public static IServiceCollection AddTriviaQueries(this IServiceCollection services)
    {
        services.AddScoped<GetRandomQuestionsQueryHandler>();
        services.AddScoped<GetCategoriesQueryHandler>();
        services.AddScoped<VerifyAnswerQueryHandler>();

        return services;
    }
}

using Microsoft.Extensions.Logging;
using TriviaApp.Domain.Model;
using TriviaApp.Domain.Workflows;

namespace TriviaApp.Domain.Extensions;

public static class TriviaIngestionWorkflowLoggerExtensions
{
    public static void LogCategoryTotals(
        this ILogger<TriviaIngestionWorkflow> logger,
        CategoryId categoryId,
        string categoryName,
        int total) =>
        logger.LogInformation(
            "Category {CategoryId} {CategoryName} total {Total}.",
            categoryId,
            categoryName,
            total);

    public static void LogEmptyResults(
        this ILogger<TriviaIngestionWorkflow> logger,
        CategoryId categoryId,
        string categoryName) =>
        logger.LogInformation(
            "Empty results for category {CategoryId} {CategoryName}.",
            categoryId,
            categoryName);

    public static void LogInsertProgress(
        this ILogger<TriviaIngestionWorkflow> logger,
        int inserted,
        int target,
        CategoryId categoryId,
        string categoryName) =>
        logger.LogInformation(
            "Inserted {Inserted}/{Target} questions for category {CategoryId} {CategoryName}.",
            inserted,
            target,
            categoryId,
            categoryName);
}

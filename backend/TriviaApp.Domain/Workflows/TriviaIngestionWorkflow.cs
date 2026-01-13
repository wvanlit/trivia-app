using System.Transactions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TriviaApp.Domain.Configuration;
using TriviaApp.Domain.Extensions;
using TriviaApp.Domain.Model;
using TriviaApp.Domain.Ports;

namespace TriviaApp.Domain.Workflows;

public sealed class TriviaIngestionWorkflow(
    ITriviaSource source,
    ITriviaRepository repository,
    IOptionsSnapshot<TriviaIngestionOptions> optionsSnapshot,
    ILogger<TriviaIngestionWorkflow> logger)
{
    private const int MaxBatchSize = 50;

    public async Task Run(CancellationToken cancellationToken)
    {
        // The worker reloads the full dataset every time.
        // Incremental updates are out of scope.
        // See `docs/ADRs.md` for more information why.
        await repository.Truncate(cancellationToken);

        var options = optionsSnapshot.Value;
        var categories = await source.GetCategories(cancellationToken);
        var categoryCounts = new Dictionary<CategoryId, int>(categories.Count);

        foreach (var category in categories)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var categoryTotal = await source.GetCategoryCount(category.Id, cancellationToken);

            categoryCounts[category.Id] = categoryTotal;

            logger.LogCategoryTotals(category.Id, category.Name, categoryTotal);
        }

        foreach (var categoryInfo in categories)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var count = categoryCounts.GetValueOrDefault(categoryInfo.Id);

            using var scope = new TransactionScope(
                scopeOption: TransactionScopeOption.Required,
                asyncFlowOption: TransactionScopeAsyncFlowOption.Enabled);

            var category = await repository.UpsertCategory(
                new Category(categoryInfo.Id, categoryInfo.Name),
                cancellationToken);

            var target = Math.Min(options.MaxQuestionsPerCategory, count);
            var inserted = 0;

            while (inserted < target)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var amount = Math.Min(MaxBatchSize, target - inserted);
                var questions = await source.GetQuestions(amount, categoryInfo.Id, cancellationToken);

                if (questions.Count == 0)
                {
                    logger.LogEmptyResults(categoryInfo.Id, categoryInfo.Name);
                    break;
                }

                await repository.InsertQuestions(category.Id, questions, cancellationToken);
                inserted += questions.Count;

                logger.LogInsertProgress(inserted, target, categoryInfo.Id, categoryInfo.Name);
            }

            // We commit every category seperately to prevent a single failure from leaving the DB without any data
            scope.Complete();
        }
    }
}

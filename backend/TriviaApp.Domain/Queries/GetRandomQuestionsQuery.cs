using TriviaApp.Domain.Model;
using TriviaApp.Domain.Ports;

namespace TriviaApp.Domain.Queries;

public sealed record GetRandomQuestionsQuery(int Count, CategoryId? CategoryId);

public sealed class GetRandomQuestionsQueryHandler(ITriviaRepository repository)
{
    private const int MaxQuestionCount = 10;

    public async Task<QueryResult<IReadOnlyList<Question>>> Handle(
        GetRandomQuestionsQuery query,
        CancellationToken cancellationToken)
    {
        if (query.Count is <= 0 or > MaxQuestionCount)
        {
            return new QueryResult<IReadOnlyList<Question>>(
                default,
                new QueryError(
                    "invalid_count",
                    $"Count must be between 1 and {MaxQuestionCount}."));
        }

        if (query.CategoryId is { } categoryId)
        {
            var exists = await repository.CategoryExists(categoryId, cancellationToken);

            if (!exists)
            {
                return new QueryResult<IReadOnlyList<Question>>(
                    default,
                    new QueryError("unknown_category", "Category does not exist."));
            }
        }

        var questions = await repository.GetRandomQuestions(query.Count, query.CategoryId, cancellationToken);

        return new QueryResult<IReadOnlyList<Question>>(questions, null);
    }
}

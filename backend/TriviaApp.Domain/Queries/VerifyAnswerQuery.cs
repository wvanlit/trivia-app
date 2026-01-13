using TriviaApp.Domain.Model;
using TriviaApp.Domain.Ports;

namespace TriviaApp.Domain.Queries;

public sealed record VerifyAnswerQuery(QuestionId QuestionId, int SelectedOptionIndex);

public sealed record VerifyAnswerResult(bool IsCorrect);

public sealed class VerifyAnswerQueryHandler(ITriviaRepository repository)
{
    public async Task<QueryResult<VerifyAnswerResult>> Handle(
        VerifyAnswerQuery query,
        CancellationToken cancellationToken)
    {
        var question = await repository.GetQuestion(query.QuestionId, cancellationToken);

        if (question is null)
        {
            return new QueryResult<VerifyAnswerResult>(
                default,
                new QueryError("unknown_question", "Question does not exist."));
        }

        var (isValid, isCorrect) = question.EvaluateAnswerIndex(query.SelectedOptionIndex);

        return !isValid
            ? new QueryResult<VerifyAnswerResult>(
                default,
                new QueryError("invalid_option_index", "Selected option index is invalid."))
            : new QueryResult<VerifyAnswerResult>(new VerifyAnswerResult(isCorrect), null);
    }
}

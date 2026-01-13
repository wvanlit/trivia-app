using TriviaApp.Domain.Model;
using TriviaApp.Domain.Queries;

using ApiResult = Microsoft.AspNetCore.Http.HttpResults.Results<
    Microsoft.AspNetCore.Http.HttpResults.Ok<TriviaApp.API.Endpoints.RandomQuestionsResponse>,
    Microsoft.AspNetCore.Http.HttpResults.BadRequest<Microsoft.AspNetCore.Mvc.ProblemDetails>>;

namespace TriviaApp.API.Endpoints;

public static class RandomQuestionsEndpoints
{
    public static RouteGroupBuilder MapRandomQuestionsEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet(
                "/questions",
                async Task<ApiResult> (
                    [AsParameters] RandomQuestionsRequest request,
                    GetRandomQuestionsQueryHandler handler,
                    CancellationToken cancellationToken) =>
                {
                    var result = await handler.Handle(request.ToDomain(), cancellationToken);

                    return result.MapToResult(RandomQuestionsResponse.FromDomain);
                })
            .WithName("GetRandomQuestions")
            .WithSummary("Gets random trivia questions.")
            .Produces<RandomQuestionsResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        return group;
    }
}

public sealed record RandomQuestionsRequest(int? Count, long? CategoryId)
{
    private const int DefaultCount = 5;

    public GetRandomQuestionsQuery ToDomain()
    {
        var count = Count ?? DefaultCount;
        var categoryId = CategoryId is { } value ? new(value) : (CategoryId?)null;

        return new GetRandomQuestionsQuery(count, categoryId);
    }
}

public sealed record RandomQuestionsResponse(IReadOnlyList<QuestionResponse> Questions)
{
    public static RandomQuestionsResponse FromDomain(IReadOnlyList<Question> questions)
        => new(questions.Select(QuestionResponse.FromDomain).ToArray());
}

public sealed record QuestionResponse(
    long QuestionId,
    long CategoryId,
    string Text,
    IReadOnlyList<string> Options,
    string Difficulty)
{
    public static QuestionResponse FromDomain(Question question)
        => new(
            question.Id.ToInt64(),
            question.CategoryId.ToInt64(),
            question.Text,
            question.Options.Select(answer => answer.Value).ToArray(),
            question.Difficulty.Value);
}

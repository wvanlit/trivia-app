using TriviaApp.Domain.Queries;

using ApiResult = Microsoft.AspNetCore.Http.HttpResults.Results<
    Microsoft.AspNetCore.Http.HttpResults.Ok<TriviaApp.API.Endpoints.VerifyAnswerResponse>,
    Microsoft.AspNetCore.Http.HttpResults.BadRequest<Microsoft.AspNetCore.Mvc.ProblemDetails>>;

namespace TriviaApp.API.Endpoints;

public static class AnswerVerificationEndpoints
{
    public static RouteGroupBuilder MapAnswerVerificationEndpoints(this RouteGroupBuilder group)
    {
        group.MapPost(
                "/questions/verify",
                async Task<ApiResult> (
                    VerifyAnswerRequest request,
                    VerifyAnswerQueryHandler handler,
                    CancellationToken cancellationToken) =>
                {
                    var result = await handler.Handle(request.ToDomain(), cancellationToken);

                    return result.MapToResult(VerifyAnswerResponse.FromDomain);
                })
            .WithName("VerifyAnswer")
            .WithSummary("Verifies whether a selected option is correct.")
            .Produces<VerifyAnswerResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        return group;
    }
}

public sealed record VerifyAnswerRequest(long QuestionId, int SelectedOptionIndex)
{
    public VerifyAnswerQuery ToDomain() => new(new(QuestionId), SelectedOptionIndex);
}

public sealed record VerifyAnswerResponse(bool IsCorrect)
{
    public static VerifyAnswerResponse FromDomain(VerifyAnswerResult result) => new(result.IsCorrect);
}

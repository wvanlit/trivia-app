using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using TriviaApp.Domain.Queries;

namespace TriviaApp.API.Endpoints;

internal static class QueryResultExtensions
{
    /// <summary>
    /// Maps a domain query result into a typed HTTP response with ProblemDetails on error.
    /// </summary>
    public static Results<Ok<TResponse>, BadRequest<ProblemDetails>> MapToResult<TDomain, TResponse>(
        this QueryResult<TDomain> result,
        Func<TDomain, TResponse> map)
    {
        return !result.IsSuccess
            ? (Results<Ok<TResponse>, BadRequest<ProblemDetails>>)TypedResults.BadRequest(CreateProblemDetails(result.Error!))
            : result.Value is null
            ? throw new InvalidOperationException("Expected a value for a successful query result.")
            : (Results<Ok<TResponse>, BadRequest<ProblemDetails>>)TypedResults.Ok(map(result.Value));
    }

    private static ProblemDetails CreateProblemDetails(QueryError error)
    {
        return new ProblemDetails
        {
            Title = error.Code,
            Detail = error.Message,
            Status = StatusCodes.Status400BadRequest
        };
    }
}

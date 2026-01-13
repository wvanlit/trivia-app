namespace TriviaApp.Domain.Queries;

public sealed record QueryError(string Code, string Message);

public sealed record QueryResult<T>(T? Value, QueryError? Error)
{
    public bool IsSuccess => Error is null;
}

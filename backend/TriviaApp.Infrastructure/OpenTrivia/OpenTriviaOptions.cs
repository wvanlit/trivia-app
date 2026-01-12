namespace TriviaApp.Infrastructure.OpenTrivia;

public sealed class OpenTriviaOptions
{
    public const string SectionName = "OpenTrivia";

    public required Uri BaseUrl { get; init; }
}

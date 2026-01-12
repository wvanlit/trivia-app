namespace TriviaApp.Domain.Configuration;

public sealed class TriviaIngestionOptions
{
    public const string SectionName = "OpenTrivia";

    public int MaxQuestionsPerCategory { get; init; }
}

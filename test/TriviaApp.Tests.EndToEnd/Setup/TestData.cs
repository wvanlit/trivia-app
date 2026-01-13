namespace TriviaApp.Tests.EndToEnd;

internal static class TestData
{
    internal const long CategoryId = 42;
    internal const string CategoryName = "Nerd Culture";
    internal const string Difficulty = "easy";

    internal static IReadOnlyList<TestQuestion> Questions { get; } =
    [
        new TestQuestion(
            "What is the name of Han Solo's ship in Star Wars?",
            ["Millennium Falcon", "X-wing", "TIE Fighter", "Star Destroyer"],
            CorrectIndex: 0,
            WrongIndex: 1,
            UseWrongAnswer: false),
        new TestQuestion(
            "What is the name of Mario's brother?",
            ["Luigi", "Wario", "Toad", "Bowser"],
            CorrectIndex: 0,
            WrongIndex: 1,
            UseWrongAnswer: true),
        new TestQuestion(
            "In The Legend of Zelda, what is the princess's name?",
            ["Zelda", "Link", "Impa", "Midna"],
            CorrectIndex: 0,
            WrongIndex: 1,
            UseWrongAnswer: false),
    ];

    internal static int GetAnswerIndex(string questionText)
    {
        return Questions.SingleOrDefault(entry => string.Equals(entry.Text, questionText, StringComparison.Ordinal)) is { } question
            ? question.UseWrongAnswer ? question.WrongIndex : question.CorrectIndex
            : throw new InvalidOperationException($"Unknown question text: {questionText}.");
    }

    internal sealed record TestQuestion(
        string Text,
        IReadOnlyList<string> Options,
        int CorrectIndex,
        int WrongIndex,
        bool UseWrongAnswer);
}

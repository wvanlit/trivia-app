using TriviaApp.Domain.Model;

namespace TriviaApp.Tests.Shared;

/// <summary>
/// Creates consistent trivia questions for tests.
/// </summary>
public static class TestQuestionFactory
{
    /// <summary>
    /// Creates a single question with fixed answers and difficulty.
    /// </summary>
    public static NewQuestion CreateQuestion(string text) =>
        new(
            text,
            new Answer("A"),
            new List<Answer> { new("A"), new("B"), new("C"), new("D") },
            new Difficulty("easy"));

    /// <summary>
    /// Creates a set of questions with a shared text prefix.
    /// </summary>
    public static IReadOnlyList<NewQuestion> CreateQuestions(int amount, string prefix)
    {
        var questions = new List<NewQuestion>(amount);
        for (var i = 0; i < amount; i++)
        {
            questions.Add(CreateQuestion($"{prefix}-{i}"));
        }

        return questions;
    }
}

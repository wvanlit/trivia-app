using TriviaApp.Domain.Model;

namespace TriviaApp.Tests.Shared;

public static class TestQuestionFactory
{
    public static NewQuestion CreateQuestion(string text) =>
        new(
            text,
            new Answer("A"),
            new List<Answer> { new("A"), new("B"), new("C"), new("D") },
            new Difficulty("easy"));

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

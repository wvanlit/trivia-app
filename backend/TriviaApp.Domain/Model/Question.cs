namespace TriviaApp.Domain.Model;

public abstract record QuestionBase(
    string Text,
    Answer CorrectAnswer,
    IReadOnlyList<Answer> Options,
    Difficulty Difficulty);

public sealed record Question(
    QuestionId Id,
    CategoryId CategoryId,
    string Text,
    Answer CorrectAnswer,
    IReadOnlyList<Answer> Options,
    Difficulty Difficulty) : QuestionBase(Text, CorrectAnswer, Options, Difficulty)
{
    /// <summary>
    /// Validates the selected option index and determines if it matches the correct answer.
    /// </summary>
    public (bool IsValid, bool IsCorrect) EvaluateAnswerIndex(int selectedOptionIndex)
    {
        if (selectedOptionIndex < 0 || selectedOptionIndex >= Options.Count)
        {
            return (false, false);
        }

        var isCorrect = Options[selectedOptionIndex] == CorrectAnswer;

        return (true, isCorrect);
    }
}

public sealed record NewQuestion(
    string Text,
    Answer CorrectAnswer,
    IReadOnlyList<Answer> Options,
    Difficulty Difficulty) : QuestionBase(Text, CorrectAnswer, Options, Difficulty);

public readonly record struct QuestionId(long Value)
{
    public static implicit operator QuestionId(long value) => new(value);

    public static QuestionId FromInt64(long value) => new(value);

    public long ToInt64() => Value;

    public static explicit operator long(QuestionId id) => id.Value;
}

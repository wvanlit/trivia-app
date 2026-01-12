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
    Difficulty Difficulty) : QuestionBase(Text, CorrectAnswer, Options, Difficulty);

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

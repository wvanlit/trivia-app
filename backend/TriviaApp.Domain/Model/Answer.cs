namespace TriviaApp.Domain.Model;

public readonly record struct Answer(string Value)
{
    public static implicit operator Answer(string value) => new(value);

    public static explicit operator string(Answer answer) => answer.Value;

    public static Answer FromString(string value) => new(value);
}

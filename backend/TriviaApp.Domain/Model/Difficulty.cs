namespace TriviaApp.Domain.Model;

public readonly record struct Difficulty(string Value)
{
    public static implicit operator Difficulty(string value) => new(value);

    public static explicit operator string(Difficulty difficulty) => difficulty.Value;

    public static Difficulty FromString(string value) => new(value);
}

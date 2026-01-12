namespace TriviaApp.Domain.Model;

public sealed record Category(CategoryId Id, string Name);

public readonly record struct CategoryId(long Value)
{
    public static implicit operator CategoryId(long value) => new(value);

    public static CategoryId FromInt64(long value) => new(value);

    public long ToInt64() => Value;

    public static explicit operator long(CategoryId id) => id.Value;
}

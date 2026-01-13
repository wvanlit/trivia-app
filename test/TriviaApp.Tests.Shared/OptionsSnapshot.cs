using Microsoft.Extensions.Options;

namespace TriviaApp.Tests.Shared;

/// <summary>
/// Provides a simple options snapshot backed by a fixed value for tests.
/// </summary>
public sealed class OptionsSnapshot<T>(T value) : IOptionsSnapshot<T>
    where T : class
{
    public T Value { get; } = value;

    public T Get(string? name) => Value;
}

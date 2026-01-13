using Microsoft.Extensions.Options;

namespace TriviaApp.Tests.Shared;

/// <summary>
/// Provides a simple options snapshot backed by a fixed value for tests.
/// </summary>
public sealed class OptionsSnapshot<T>(T value) : IOptionsSnapshot<T>
    where T : class
{
    /// <summary>
    /// Gets the configured options value.
    /// </summary>
    public T Value { get; } = value;

    /// <summary>
    /// Returns the configured options value for any name.
    /// </summary>
    public T Get(string? name) => Value;
}

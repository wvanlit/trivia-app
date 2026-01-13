using TriviaApp.Domain.Model;
using TriviaApp.Domain.Ports;

namespace TriviaApp.Tests.Shared;

/// <summary>
/// Provides a configurable trivia source for tests.
/// </summary>
public sealed class FakeTriviaSource(
    IReadOnlyList<Category> categories,
    IReadOnlyDictionary<CategoryId, int> categoryCounts,
    Func<int, CategoryId, IReadOnlyList<NewQuestion>>? questionsFactory = null) : ITriviaSource
{
    private readonly Func<int, CategoryId, IReadOnlyList<NewQuestion>> _questionsFactory =
        questionsFactory ?? ((amount, categoryId) => TestQuestionFactory.CreateQuestions(amount, $"Q-{categoryId.ToInt64()}"));

    /// <summary>
    /// Returns the configured categories.
    /// </summary>
    public Task<IReadOnlyList<Category>> GetCategories(CancellationToken cancellationToken) =>
        Task.FromResult(categories);

    /// <summary>
    /// Returns the configured category question count.
    /// </summary>
    public Task<int> GetCategoryCount(CategoryId categoryId, CancellationToken cancellationToken) =>
        Task.FromResult(categoryCounts[categoryId]);

    /// <summary>
    /// Returns questions from the configured factory.
    /// </summary>
    public Task<IReadOnlyList<NewQuestion>> GetQuestions(
        int amount,
        CategoryId categoryId,
        CancellationToken cancellationToken) =>
        Task.FromResult(_questionsFactory(amount, categoryId));
}

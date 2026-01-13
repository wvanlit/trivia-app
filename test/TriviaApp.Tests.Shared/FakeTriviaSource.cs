using TriviaApp.Domain.Model;
using TriviaApp.Domain.Ports;

namespace TriviaApp.Tests.Shared;

public sealed class FakeTriviaSource(
    IReadOnlyList<Category> categories,
    IReadOnlyDictionary<CategoryId, int> categoryCounts,
    Func<int, CategoryId, IReadOnlyList<NewQuestion>>? questionsFactory = null) : ITriviaSource
{
    private readonly Func<int, CategoryId, IReadOnlyList<NewQuestion>> _questionsFactory =
        questionsFactory ?? ((amount, categoryId) => TestQuestionFactory.CreateQuestions(amount, $"Q-{categoryId.ToInt64()}"));

    public Task<IReadOnlyList<Category>> GetCategories(CancellationToken cancellationToken) =>
        Task.FromResult(categories);

    public Task<int> GetCategoryCount(CategoryId categoryId, CancellationToken cancellationToken) =>
        Task.FromResult(categoryCounts[categoryId]);

    public Task<IReadOnlyList<NewQuestion>> GetQuestions(
        int amount,
        CategoryId categoryId,
        CancellationToken cancellationToken) =>
        Task.FromResult(_questionsFactory(amount, categoryId));
}

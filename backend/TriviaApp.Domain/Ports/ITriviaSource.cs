using TriviaApp.Domain.Model;

namespace TriviaApp.Domain.Ports;

public interface ITriviaSource
{
    Task<IReadOnlyList<Category>> GetCategories(CancellationToken cancellationToken);

    Task<int> GetCategoryCount(CategoryId categoryId, CancellationToken cancellationToken);

    Task<IReadOnlyList<NewQuestion>> GetQuestions(
        int amount,
        CategoryId categoryId,
        CancellationToken cancellationToken);
}

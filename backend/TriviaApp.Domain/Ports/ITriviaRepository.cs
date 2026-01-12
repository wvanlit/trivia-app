using TriviaApp.Domain.Model;

namespace TriviaApp.Domain.Ports;

public interface ITriviaRepository
{
    Task Truncate(CancellationToken cancellationToken);

    Task<Category> UpsertCategory(Category category, CancellationToken cancellationToken);

    Task<int> InsertQuestions(CategoryId categoryId, IReadOnlyList<NewQuestion> questions, CancellationToken cancellationToken);
}

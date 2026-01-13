using TriviaApp.Domain.Model;

namespace TriviaApp.Domain.Ports;

public interface ITriviaRepository
{
    Task Truncate(CancellationToken cancellationToken);

    Task<Category> UpsertCategory(Category category, CancellationToken cancellationToken);

    Task<int> InsertQuestions(CategoryId categoryId, IReadOnlyList<NewQuestion> questions, CancellationToken cancellationToken);

    Task<IReadOnlyList<Question>> GetRandomQuestions(int count, CategoryId? categoryId, CancellationToken cancellationToken);

    Task<IReadOnlyList<Category>> GetCategories(CancellationToken cancellationToken);

    Task<Question?> GetQuestion(QuestionId questionId, CancellationToken cancellationToken);

    Task<bool> CategoryExists(CategoryId categoryId, CancellationToken cancellationToken);
}

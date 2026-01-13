using TriviaApp.Domain.Model;
using TriviaApp.Domain.Ports;

namespace TriviaApp.Tests.Shared;

public sealed class FakeTriviaRepository : ITriviaRepository
{
    private readonly List<string> _calls = [];
    private readonly List<Category> _upserted = [];
    private readonly List<(CategoryId CategoryId, IReadOnlyList<NewQuestion> Questions)> _inserts = [];

    public IReadOnlyList<string> Calls => _calls;

    public IReadOnlyList<Category> Upserted => _upserted;

    public IReadOnlyList<(CategoryId CategoryId, IReadOnlyList<NewQuestion> Questions)> Inserts => _inserts;

    public Task Truncate(CancellationToken cancellationToken)
    {
        _calls.Add("Truncate");
        return Task.CompletedTask;
    }

    public Task<Category> UpsertCategory(Category category, CancellationToken cancellationToken)
    {
        _calls.Add("Upsert");
        _upserted.Add(category);

        return Task.FromResult(category);
    }

    public Task<int> InsertQuestions(
        CategoryId categoryId,
        IReadOnlyList<NewQuestion> questions,
        CancellationToken cancellationToken)
    {
        _calls.Add("Insert");
        _inserts.Add((categoryId, questions));

        return Task.FromResult(questions.Count);
    }

    public Task<IReadOnlyList<Question>> GetRandomQuestions(
        int count,
        CategoryId? categoryId,
        CancellationToken cancellationToken)
    {
        _calls.Add("GetRandomQuestions");
        return Task.FromResult<IReadOnlyList<Question>>([]);
    }

    public Task<IReadOnlyList<Category>> GetCategories(CancellationToken cancellationToken)
    {
        _calls.Add("GetCategories");
        return Task.FromResult<IReadOnlyList<Category>>([]);
    }

    public Task<Question?> GetQuestion(QuestionId questionId, CancellationToken cancellationToken)
    {
        _calls.Add("GetQuestion");
        return Task.FromResult<Question?>(null);
    }

    public Task<bool> CategoryExists(CategoryId categoryId, CancellationToken cancellationToken)
    {
        _calls.Add("CategoryExists");
        return Task.FromResult(false);
    }
}
